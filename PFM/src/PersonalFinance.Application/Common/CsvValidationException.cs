using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinance.Application.Common
{
    public class CsvValidationException : Exception
    {
        public List<ValidationError> Errors { get; }

        public CsvValidationException(IEnumerable<ValidationError> errors)
            : base("Greške pri validaciji CSV fajla")
        {
            Errors = errors.ToList();
        }
    }
}
