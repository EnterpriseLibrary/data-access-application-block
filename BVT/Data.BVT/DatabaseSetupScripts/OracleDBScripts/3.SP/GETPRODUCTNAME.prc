CREATE OR REPLACE PROCEDURE GetProductName
(vProductID IN Products.ProductID%TYPE := 1,vResult OUT Products.ProductName%TYPE)
AS

BEGIN
	SELECT ProductName INTO vResult FROM Products where ProductId = vProductId;
END;
/

