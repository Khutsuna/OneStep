namespace OneStep.Application.Common;

/// <summary>
/// Turns a stored relative file path into the URL the browser loads, e.g.
/// "products/12/a.jpg" -> "/uploads/products/12/a.jpg". The Angular app puts the API address in front.
/// </summary>
public static class MediaUrl
{
    public static string? From(string? relativePath)
    {
        return string.IsNullOrEmpty(relativePath) ? null : "/uploads/" + relativePath.Replace('\\', '/');
    }
}
