using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpellParser;

public static class ListExtensions
{
    public static string GetAndRemove(this IList<string> list, string regexPattern)
    {
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        var s = list.FirstOrDefault(s => regex.IsMatch(s));
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;

        list.Remove(s);
        return regex.Replace(s, "").Trim();
    }
}