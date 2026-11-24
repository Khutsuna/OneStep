/* =====================================================================
   OneStep - stored procedures for the admin dashboard (sales analytics).
   Run after `dotnet ef database update` (EF creates the tables).
   The API calls them from Infrastructure/Persistence/Queries/SqlAnalyticsQueries.cs.

   "Revenue" = orders that were paid: status Paid, Shipped or Delivered.
   Pending (not paid yet) and Cancelled orders don't count as money earned.
   Dates are UTC, like everything the API stores. @To is exclusive (< @To).
   ===================================================================== */
USE OneStep;
GO

/* ---------------------------------------------------------------------
   1. Key numbers for the period, in one row.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_GetDashboardSummary
    @From datetime2,
    @To   datetime2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Money earned in the period
        Revenue        = ISNULL((SELECT SUM(o.TotalAmount)
                                 FROM dbo.Orders AS o
                                 WHERE o.CreatedAt >= @From AND o.CreatedAt < @To
                                   AND o.Status IN ('Paid', 'Shipped', 'Delivered')), 0),

        PaidOrderCount = (SELECT COUNT(*)
                          FROM dbo.Orders AS o
                          WHERE o.CreatedAt >= @From AND o.CreatedAt < @To
                            AND o.Status IN ('Paid', 'Shipped', 'Delivered')),

        -- Every order placed in the period that wasn't cancelled
        OrderCount     = (SELECT COUNT(*)
                          FROM dbo.Orders AS o
                          WHERE o.CreatedAt >= @From AND o.CreatedAt < @To
                            AND o.Status <> 'Cancelled'),

        NewCustomers   = (SELECT COUNT(*)
                          FROM dbo.Users AS u
                          WHERE u.Role = 'Customer'
                            AND u.CreatedAt >= @From AND u.CreatedAt < @To),

        -- "Needs attention" right now, whatever the period
        PendingOrders  = (SELECT COUNT(*) FROM dbo.Orders WHERE Status = 'Pending'),
        OrdersToShip   = (SELECT COUNT(*) FROM dbo.Orders WHERE Status = 'Paid'),
        LowStockProducts = (SELECT COUNT(*)
                            FROM dbo.Products AS p
                            WHERE p.IsDeleted = 0
                              AND p.Status = 'Active'
                              AND p.StockQuantity <= p.LowStockThreshold);
END;
GO

/* ---------------------------------------------------------------------
   2. Revenue per day, for the sales chart.
      Days without sales have no row here; the API fills them with zero.
      [You] Gio writes this one.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_GetSalesByDay
    @From datetime2,
    @To   datetime2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST(o.CreatedAt AS date) AS [Day],
        COUNT(*)                  AS OrderCount,
        SUM(o.TotalAmount)        AS Revenue
    FROM dbo.Orders AS o
    WHERE o.CreatedAt >= @From
      AND o.CreatedAt <  @To
      AND o.Status IN ('Paid', 'Shipped', 'Delivered')
    GROUP BY CAST(o.CreatedAt AS date)
    ORDER BY [Day];
END;
GO

/* ---------------------------------------------------------------------
   3. Best-selling products in the period, by revenue.
      Uses the order lines' copied names, so deleted products still show.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_GetTopProducts
    @From datetime2,
    @To   datetime2,
    @Top  int = 5
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Top)
        oi.ProductId,
        ProductName = MAX(oi.ProductName),
        UnitsSold   = SUM(oi.Quantity),
        Revenue     = SUM(oi.LineTotal)
    FROM dbo.OrderItems AS oi
    INNER JOIN dbo.Orders AS o ON o.Id = oi.OrderId
    WHERE o.CreatedAt >= @From AND o.CreatedAt < @To
      AND o.Status IN ('Paid', 'Shipped', 'Delivered')
    GROUP BY oi.ProductId
    ORDER BY Revenue DESC, UnitsSold DESC;
END;
GO

/* Try them:
   DECLARE @to datetime2 = DATEADD(day, 1, CAST(SYSUTCDATETIME() AS date));
   DECLARE @from datetime2 = DATEADD(day, -30, @to);
   EXEC dbo.usp_GetDashboardSummary @From = @from, @To = @to;
   EXEC dbo.usp_GetSalesByDay      @From = @from, @To = @to;
   EXEC dbo.usp_GetTopProducts     @From = @from, @To = @to, @Top = 5;
*/
