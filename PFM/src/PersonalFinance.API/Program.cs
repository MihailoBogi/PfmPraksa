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
builder.Services.AddScoped<ISplitService, SplitService>();
builder.Services.AddScoped<IAutoCategorizationService, AutoCategorizationService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CsvValidationFilter>();
    options.Filters.Add<BusinessExceptionFilter>();
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
            .SelectMany(kvp => kvp.Value.Errors.Select(err => new ValidationError
            {
                Tag = kvp.Key,
                Error = ValidationErrorCode.InvalidFormat,
                Message = err.ErrorMessage
            }))
            .ToList();

        return new BadRequestObjectResult(
            new ValidationErrorResponse { Errors = errors }
        );
    };
});

var app = builder.Build();

// middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalFinance API v1");
        // c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
