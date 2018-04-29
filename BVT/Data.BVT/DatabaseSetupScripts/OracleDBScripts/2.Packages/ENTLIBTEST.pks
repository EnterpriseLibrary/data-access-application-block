CREATE OR REPLACE PACKAGE EntlibTest AS

PROCEDURE GetProductDetailsById
(vProductID IN NUMBER,vProductName OUT VARCHAR2,vUnitPrice OUT NUMBER);

END EntlibTest;
/

