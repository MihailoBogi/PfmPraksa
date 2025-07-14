using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class TransactionCategorizeCommand
    {
        [Required]
        [JsonPropertyName("catcode")]
        public string CatCode { get; set; } = default!;
    }
}
