CREATE OR ALTER VIEW dbo.vw_ProductCatalog
AS
SELECT
    p.Id            AS ProductId,
    p.Name,
    p.Slug,
    p.Sku,
    p.Price,
    p.StockQuantity,
    p.Status,
    c.Id            AS CategoryId,
    c.Name          AS CategoryName,
    b.Id            AS BrandId,
    b.Name          AS BrandName,
    img.FilePath    AS MainImagePath,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    ISNULL(r.ReviewCount, 0)   AS ReviewCount,
    p.CreatedAt
FROM dbo.Products AS p
INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
LEFT JOIN dbo.Brands AS b ON b.Id = p.BrandId
LEFT JOIN dbo.ProductImages AS img ON img.ProductId = p.Id AND img.IsMain = 1
LEFT JOIN (
    SELECT
        ProductId,
        CAST(AVG(CAST(Rating AS decimal(3, 2))) AS decimal(3, 2)) AS AverageRating,
        COUNT(*) AS ReviewCount
    FROM dbo.Reviews
    GROUP BY ProductId
) AS r ON r.ProductId = p.Id
WHERE p.IsDeleted = 0;