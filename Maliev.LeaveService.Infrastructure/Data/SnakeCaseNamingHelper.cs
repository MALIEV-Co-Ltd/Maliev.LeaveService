using System.Text.RegularExpressions;

namespace Maliev.LeaveService.Infrastructure.Data;

public static class SnakeCaseNamingHelper
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscore = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");
        return startUnderscore.ToLower();
    }
}
