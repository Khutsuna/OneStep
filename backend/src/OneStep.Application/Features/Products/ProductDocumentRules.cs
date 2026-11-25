using System.Text;

namespace OneStep.Application.Features.Products;

/// <summary>
/// The rules for product documents (PDF manuals), as pure functions so they can be unit tested
/// (see ProductDocumentRulesTests).
/// </summary>
public static class ProductDocumentRules
{
    public const long MaxFileSize = 20 * 1024 * 1024;   // 20 MB
    public const int MaxDocumentsPerProduct = 10;
    public const int MaxTitleLength = 150;
    private const int MaxFileNameLength = 100;

    /// <summary>Every PDF file starts with these five bytes.</summary>
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    public static bool HasPdfExtension(string fileName) =>
        string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks the file's first bytes, not only its name. An HTML page renamed to "manual.pdf"
    /// would otherwise be served from our own site, where its scripts could run (stored XSS).
    /// </summary>
    public static bool LooksLikePdf(ReadOnlySpan<byte> start) => start.StartsWith(PdfSignature);

    /// <summary>The title customers see: what staff typed, or else the file name without ".pdf".</summary>
    public static string MakeTitle(string? title, string fileName)
    {
        var text = string.IsNullOrWhiteSpace(title)
            ? Path.GetFileNameWithoutExtension(SafeFileName(fileName))
            : title.Trim();
        return text.Length > MaxTitleLength ? text[..MaxTitleLength].TrimEnd() : text;
    }

    /// <summary>
    /// The uploader's file name, made safe to show: no folders, only letters (Georgian too), digits,
    /// spaces and - _ . ( ), at most 100 characters, always ending in ".pdf".
    /// The file on disk never uses this name; it gets a random one.
    /// </summary>
    public static string SafeFileName(string fileName)
    {
        var name = fileName.Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..];
        if (HasPdfExtension(name))
        {
            name = name[..^4];
        }

        var clean = new StringBuilder();
        foreach (var c in name)
        {
            clean.Append(char.IsLetterOrDigit(c) || " -_.()".Contains(c) ? c : '-');
        }

        var result = clean.ToString().Trim(' ', '-', '.');
        if (result.Length > MaxFileNameLength)
        {
            result = result[..MaxFileNameLength].TrimEnd(' ', '-', '.');
        }
        return (result.Length == 0 ? "document" : result) + ".pdf";
    }
}
