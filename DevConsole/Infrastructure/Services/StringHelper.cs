using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevConsole.Infrastructure.Services;

public static class StringHelper
{
    public static string RemoveInvalidCharacters(string input, IDictionary<Regex, string>? patterns = null)
    {
        input = input.Trim();
        if (patterns is not null)
        {
            input = patterns.Aggregate(input, (current, pair) => pair.Key.Replace(current, pair.Value));
        }

        const string invalidCharactersPattern = @"[\.!\?`]|[^\x00-\x7F]";
        return Regex.Replace(input, invalidCharactersPattern, string.Empty);
    }
}