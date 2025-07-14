using CsvHelper.Configuration;
using PersonalFinance.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Mapping
{
    public sealed class ImportCategoryDtoMap : ClassMap<ImportCategoryDto>
    {
        public ImportCategoryDtoMap() 
        {
            Map(m => m.Code).Name("code");
            Map(m => m.Name).Name("name");
            Map(m => m.ParentCode).Name("parent-code");
        }
    }
}
