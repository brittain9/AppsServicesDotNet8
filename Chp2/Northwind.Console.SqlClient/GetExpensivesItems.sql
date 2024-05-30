USE Northwind; -- Switch to Northwind db instead of master
GO

CREATE PROCEDURE [dbo].[GetExpensiveProducts2]
    @price money,
    @count int OUT
AS
    PRINT 'Getting expensive products: ' + 
        TRIM(CAST(@price AS NVARCHAR(10)))
    
    SELECT @count = COUNT(*)
    FROM Northwind.dbo.Products
        WHERE UnitPrice >= @price

    SELECT *
    FROM Northwind.dbo.Products
    WHERE UnitPrice >= @price

RETURN 0