CREATE OR REPLACE PROCEDURE ADDEMPLOYEEGETCOUNT
(vEmployeeID IN Employees.EmployeeID%TYPE,
 vFirstName IN Employees.FirstName%TYPE,
 vLastName IN Employees.LastName%TYPE,
 vReportsTo IN Employees.ReportsTo%TYPE,
 vEmployeeCount OUT INT)
AS
BEGIN
	INSERT INTO Employees (EmployeeID, FirstName, LastName, ReportsTo)
	 VALUES (vEmployeeID, vFirstName, vLastName, vReportsTo);
	 SELECT COUNT(*) INTO vEmployeeCount FROM EMPLOYEES;
END;
/

