using System.Text;

namespace ManageMyMoney.Infrastructure.Persistence.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(input[0]));

        for (var i = 1; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
