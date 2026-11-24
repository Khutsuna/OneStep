/*
  OneStep - demo price history  (07_demo_price_history.sql)
  Run in SSMS after 06_seed.sql. Safe to run more than once: it only inserts when
  these products have no price history yet.

  In the real app these rows are written by the trigger trg_Products_PriceHistory
  (05_triggers.sql) whenever a manager changes a price. Here we add a few past changes,
  so the price chart on the product page has something to show. The last NewPrice of
  each product is its current price in 06_seed.sql.
*/
USE OneStep;
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.ProductPriceHistory WHERE ProductId IN (2, 5, 13, 23, 25))
BEGIN
    PRINT 'Demo price history already exists. Nothing to do.';
    RETURN;
END;

DECLARE @Now datetime2 = SYSUTCDATETIME();

INSERT INTO dbo.ProductPriceHistory (ProductId, OldPrice, NewPrice, CreatedAt) VALUES
    (2,  4399.00, 4299.00, DATEADD(MINUTE, -120000, @Now)),   -- iPhone 16 Pro 256GB
    (2,  4299.00, 4199.00, DATEADD(MINUTE,  -70000, @Now)),
    (5,  4299.00, 4199.00, DATEADD(MINUTE, -110000, @Now)),   -- Galaxy S25 Ultra 256GB
    (5,  4199.00, 4099.00, DATEADD(MINUTE,  -60000, @Now)),
    (13, 3999.00, 3899.00, DATEADD(MINUTE, -100000, @Now)),   -- MacBook Air 13" M4
    (13, 3899.00, 3799.00, DATEADD(MINUTE,  -30000, @Now)),
    (23,  799.00,  749.00, DATEADD(MINUTE,  -90000, @Now)),   -- AirPods Pro 2
    (25, 1299.00, 1199.00, DATEADD(MINUTE,  -95000, @Now)),   -- Sony WH-1000XM5
    (25, 1199.00, 1099.00, DATEADD(MINUTE,  -50000, @Now));

PRINT 'Demo price history added.';
