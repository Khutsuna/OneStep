using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public byte Rating { get; set; }                            // 1 to 5
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
