using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PersonalFinance.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        public CategoryService(ApplicationDbContext db) => _db = db;
        public async Task<List<CategoryDto>> GetByParentCodeAsync(string? parentCode)
        {
            var errors = new List<ValidationError>();
            if (parentCode != null && parentCode.Length > 20)
            {
                errors.Add(new ValidationError
                {
                    Tag = "parent-code",
                    Error = ValidationErrorCode.MaxLength,
                    Message = $"Given parent-code '{parentCode}' exceeds 20 characters."
                });
            }
            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            //var categories = await _db.Categories
            //    .Where(c => c.ParentCode == parentCode)
            //    .Select(c => new CategoryDto
            //    {
            //        Code = c.Code,
            //        Name = c.Name,
            //        ParentCode = c.ParentCode
            //    })
            //    .ToListAsync();

            //return categories;
            var query = _db.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(parentCode))
                query = query.Where(c => c.ParentCode == parentCode);

            var categories = await query
              .Select(c => new CategoryDto { 
                    Code = c.Code, Name = c.Name,ParentCode = c.ParentCode
              })
              .ToListAsync();
            return categories;
        }
    }
}
