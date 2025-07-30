using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PersonalFinance.API.Common;
using PersonalFinance.API.Common.Filters;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Infrastructure.Data;
using PersonalFinance.Infrastructure.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);//dozvoljna za kind za timezone unspecified
builder.Configuration
       .AddJsonFile("auto-categorization-rules.json", optional: false, reloadOnChange: true);

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions
            .UseRelationalNulls()
    )
);
builder.Services.Configure<AutoCategorizationOptions>(builder.Configuration);

builder.Services.AddScoped<ITransactionImporter, TransactionImporter>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryImporter, CategoryImporter>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISplitService, SplitService>();
builder.Services.AddScoped<IAutoCategorizationService, AutoCategorizationService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CsvValidationFilter>();
    options.Filters.Add<BusinessExceptionFilter>();
})
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = new KebabCaseNamingPolicy();
        opts.JsonSerializerOptions.DictionaryKeyPolicy = new KebabCaseNamingPolicy();
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PersonalFinance API",
        Version = "v1",
        Description = "Endpoints for importing and querying transactions"
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value.Errors.Any())
            .SelectMany(kvp => kvp.Value.Errors.Select(err =>
            {
                // MAPIRANJE tipa greške
                var code = ValidationErrorCode.InvalidFormat;
                if (err.ErrorMessage.Contains("required"))
                    code = ValidationErrorCode.Required;
                else if (err.ErrorMessage.Contains("between")
                      || err.ErrorMessage.Contains("must be"))
                    code = ValidationErrorCode.OutOfRange;
                else if (err.Exception is FormatException)
                    code = ValidationErrorCode.InvalidFormat;

                return new ValidationError
                {
                    Tag = kvp.Key,
                    Error = code,
                    Message = err.ErrorMessage
                };
            }))
            .ToList();

        return new BadRequestObjectResult(new ValidationErrorResponse { Errors = errors });
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

// middleware
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService< ApplicationDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalFinance API V1");
});
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
