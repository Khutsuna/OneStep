using System.Text.RegularExpressions;

namespace OneStep.Application.Common;

/// <summary>Builds URL-friendly names: "iPhone 16 Pro 256GB" -> "iphone-16-pro-256gb".</summary>
public static partial class SlugHelper
{
    public static string Create(string text)
    {
        var slug = NonAlphanumeric().Replace(text.Trim().ToLowerInvariant(), "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    /// <summary>Adds -2, -3, ... until the slug is not taken yet.</summary>
    public static async Task<string> CreateUniqueAsync(
        string text, Func<string, Task<bool>> isTaken)
    {
        var baseSlug = Create(text);
        var candidate = baseSlug;
        var counter = 2;
        while (await isTaken(candidate))
        {
            candidate = $"{baseSlug}-{counter++}";
        }
        return candidate;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
