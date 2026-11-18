CREATE OR ALTER TRIGGER dbo.trg_Products_PriceHistory
ON dbo.Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ProductPriceHistory (ProductId, OldPrice, NewPrice)
    SELECT d.Id, d.Price, i.Price
    FROM inserted AS i
    INNER JOIN deleted AS d ON d.Id = i.Id
    WHERE i.Price <> d.Price;
END;