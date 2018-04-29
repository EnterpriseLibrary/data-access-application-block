CREATE OR REPLACE PROCEDURE GetProductsView
( cur_OUT OUT PKGENTLIB_ARCHITECTURE.CURENTLIB_ARCHITECTURE)
AS

BEGIN
    OPEN cur_OUT FOR
	SELECT ProductId as Id,ProductName as Name, CategoryId as Category,UnitPrice, LastUpdate  FROM Products where ProductId in (1,2,3,4,5)	ORDER BY ProductID;
END;
/
