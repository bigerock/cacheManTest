using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class StringExtensions
{
    public static string ToLowerObj(this object str)
    {
        return str?.ToString().ToLowerInvariant() ?? string.Empty;
    }
}


