using System.Text.Json;
using System.Text.RegularExpressions;

namespace PersonalFinance.API.Common
{
    public class KebabCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var kebab = Regex
                .Replace(name, "([a-z0-9])([A-Z])", "$1-$2")
                .Replace("_", "-")
                .ToLowerInvariant();

            return kebab;
        }
    }
}
