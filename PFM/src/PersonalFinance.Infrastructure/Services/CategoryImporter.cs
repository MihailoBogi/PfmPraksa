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

            //var existing = await _db.Categories
            //    .ToDictionaryAsync(c => c.Code, c => c);

            //foreach (var d in dtos)
            //{
            //    if (existing.TryGetValue(d.Code, out var cat))
            //    {
            //        cat.Update(d.Name, d.ParentCode);
            //    }
            //    else
            //    {
            //        var newCat = new Category(d.Code, d.Name, d.ParentCode);
            //        await _db.Categories.AddAsync(newCat);
            //        existing[d.Code] = newCat;
            //    }
            //}

            //await _db.SaveChangesAsync();
            // 2) Build a lookup of all *existing* codes in the DB
            var existing = await _db.Categories
                .AsNoTracking()
                .Select(c => c.Code)
                .ToListAsync();

            // 3) Also include any *new* codes from this batch,
            //    so you can import parent & child in same file.
            var batchCodes = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
            foreach (var d in dtos)
                batchCodes.Add(d.Code);

            // 4) Now catch any dto whose ParentCode is non-null but not in batchCodes
            foreach (var (d, idx) in dtos.Select((dto, i) => (dto, i + 2 /* data row in CSV */)))
            {
                if (d.ParentCode is not null && !batchCodes.Contains(d.ParentCode))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "parent-code",
                        Error = ValidationErrorCode.NotOnList,
                        Message = $"Red {idx}: ne postoji kategorija '{d.ParentCode}'"
                    });
                }
            }

            // 5) Bail out if parent-code errors found
            if (errors.Any())
                throw new CsvValidationException(errors);

            // 6) At this point you know every ParentCode is valid or null,
            //    so you can safely insert or update
            var categories = await _db.Categories
                .ToDictionaryAsync(c => c.Code, c => c);

            foreach (var d in dtos)
            {
                if (categories.TryGetValue(d.Code, out var cat))
                {
                    cat.Update(d.Name, d.ParentCode);
                }
                else
                {
                    var newCat = new Category(d.Code, d.Name, d.ParentCode);
                    await _db.Categories.AddAsync(newCat);
                    categories[d.Code] = newCat;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
