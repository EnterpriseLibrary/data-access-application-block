CREATE OR REPLACE PROCEDURE "GETCUSTOMEROUT" ( vCustomerId IN Customers.CustomerID%TYPE, vName OUT Customers.COMPANYNAME%TYPE,cur_OUT OUT PKGENTLIB_ARCHITECTURE.CURENTLIB_ARCHITECTURE)
	AS

BEGIN
    OPEN cur_OUT FOR
	SELECT * FROM Customers where customerID = vCustomerId;
	SELECT CompanyName INTO vName FROM Customers where customerID = vCustomerId;
END;
/

