using System.Text.RegularExpressions;

namespace Backend.Application.Common;

/// <summary>
/// Shared slug normalization logic for portfolio slugs.
/// Rules: trim → lowercase → spaces to hyphens → keep [a-z0-9-] → collapse hyphens → trim hyphens.
/// </summary>
public static class SlugHelper
{
    public static string NormalizeSlug(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var s = raw.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"[^a-z0-9\-]", "");
        s = Regex.Replace(s, @"-{2,}", "-");
        s = s.Trim('-');

        return s;
    }
}
