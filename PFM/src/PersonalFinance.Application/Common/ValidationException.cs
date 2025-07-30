using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Common
{
    public class ValidationException : Exception
    {
        public List<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationError> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors.ToList();
        }
    }
}
