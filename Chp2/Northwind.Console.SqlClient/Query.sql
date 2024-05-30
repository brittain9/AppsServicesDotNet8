SELECT 
    ProductName, 
    (UnitPrice * UnitsInStock) AS "Total Stock Value"
FROM Northwind.dbo.[Products]
WHERE (UnitPrice * UnitsInStock > 0);


DECLARE @customersCount INT;
SET @customersCount = @@ROWCOUNT;
PRINT('Row count: ');
PRINT(@customersCount);