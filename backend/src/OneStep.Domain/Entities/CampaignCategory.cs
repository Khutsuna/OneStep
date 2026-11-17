namespace OneStep.Domain.Entities;

/// <summary>Link table: which categories a campaign applies to. Primary key: (CampaignId, CategoryId).</summary>
public class CampaignCategory
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
