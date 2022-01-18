using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellParser;

public static class ListExtensions
{
    public static string GetAndRemove(this IList<string> list, string stringToFind)
    {
        var s = list.FirstOrDefault(s => s.StartsWith(stringToFind, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;

        list.Remove(s);
        return s.Replace(stringToFind, "", StringComparison.OrdinalIgnoreCase);
    }
}