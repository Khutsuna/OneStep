namespace OneStep.Domain.Entities;

/// <summary>Link table: which products a campaign applies to. Primary key: (CampaignId, ProductId).</summary>
public class CampaignProduct
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
