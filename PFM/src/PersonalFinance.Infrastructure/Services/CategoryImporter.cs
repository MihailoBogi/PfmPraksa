using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data;
using PersonalFinance.Infrastructure.Mapping;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Services
{
    public class CategoryImporter : ICategoryImporter
    {
        private readonly ApplicationDbContext _db;
        public CategoryImporter(ApplicationDbContext db) => _db = db;

        public async Task ImportAsync(IFormFile csvFile)
        {
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                PrepareHeaderForMatch = args => args.Header.Trim(),
                HeaderValidated = null,
                MissingFieldFound = null
            };
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ImportCategoryDtoMap>();

            csv.Read();
            csv.ReadHeader();

            var errors = new List<ValidationError>();
            var dtos = new List<ImportCategoryDto>();
            var row = 1;

            while (csv.Read())
            {
                row++;
                ImportCategoryDto dto;
                try
                {
                    dto = csv.GetRecord<ImportCategoryDto>();
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError
                    {
                        Tag = $"row-{row}",
                        Error = ValidationErrorCode.InvalidFormat,
                        Message = $"Red {row}: ne mogu parsirati red: {ex.GetBaseException().Message}"
                    });
                    continue;
                }
                var parent = dto.ParentCode?.Trim();
                dto.ParentCode = string.IsNullOrWhiteSpace(parent)
                    ? null
                    : parent!;
                if (string.IsNullOrWhiteSpace(dto.Code))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "code",
                        Error = ValidationErrorCode.Required,
                        Message = $"Red {row}: 'code' je obavezan"
                    });
                    continue;
                }
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "name",
                        Error = ValidationErrorCode.Required,
                        Message = $"Red {row}: 'name' je obavezan"
                    });
                    continue;
                }

                dtos.Add(dto);
            }

            if (errors.Any())
                throw new CsvValidationException(errors);

            // Fetch existing categories by code
            var existing = await _db.Categories
                .ToDictionaryAsync(c => c.Code, c => c);

            foreach (var d in dtos)
            {
                if (existing.TryGetValue(d.Code, out var cat))
                {
                    // update name/parent
                    cat.Update(d.Name, d.ParentCode);
                }
                else
                {
                    var newCat = new Category(d.Code, d.Name, d.ParentCode);
                    await _db.Categories.AddAsync(newCat);
                    existing[d.Code] = newCat;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
