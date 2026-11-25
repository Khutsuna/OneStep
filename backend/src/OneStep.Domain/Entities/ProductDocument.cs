using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>A downloadable PDF for a product, such as its user manual or warranty card.</summary>
public class ProductDocument : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Title { get; set; } = string.Empty;       // shown to customers, e.g. "User manual"
    public string FileName { get; set; } = string.Empty;    // the uploader's file name, cleaned: "WH-1000XM5 manual.pdf"
    public string FilePath { get; set; } = string.Empty;    // where it is stored, under a random name: "documents/25/3f9c....pdf"
    public long SizeBytes { get; set; }
}
