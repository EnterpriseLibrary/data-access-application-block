CREATE OR REPLACE PACKAGE BODY EntlibTest AS

PROCEDURE GetProductDetailsById
(vProductID IN NUMBER,vProductName OUT VARCHAR2,vUnitPrice OUT NUMBER)
AS

BEGIN
	SELECT ProductName,UnitPrice INTO vProductName,vUnitPrice FROM Products where ProductId = vProductId;
END;

END EntlibTest;
/

