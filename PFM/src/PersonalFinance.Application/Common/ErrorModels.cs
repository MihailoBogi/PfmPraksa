using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace PersonalFinance.Application.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ValidationErrorCode
    {
        [EnumMember(Value = "min-length")] MinLength,
        [EnumMember(Value = "max-length")] MaxLength,
        [EnumMember(Value = "required")] Required,
        [EnumMember(Value = "out-of-range")] OutOfRange,
        [EnumMember(Value = "invalid-format")] InvalidFormat,
        [EnumMember(Value = "unknown-enum")] UnknownEnum,
        [EnumMember(Value = "not-on-list")] NotOnList,
        [EnumMember(Value = "check-digit-invalid")] CheckDigitInvalid,
        [EnumMember(Value = "combination-required")] CombinationRequired,
        [EnumMember(Value = "read-only")] ReadOnly
    }

    public class ValidationError
    {
        public string Tag { get; set; } = default!;
        public ValidationErrorCode Error { get; set; }
        public string Message { get; set; } = default!;
    }

    public class ValidationErrorResponse
    {
        public List<ValidationError> Errors { get; set; } = new();
    }

    public class BusinessErrorResponse
    {
        public string Problem { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Details { get; set; } = default!;
    }
}
