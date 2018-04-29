CREATE OR REPLACE PROCEDURE ADDEMPLOYEES
(vEmployeeID IN Employees.EmployeeID%TYPE,
 vFirstName IN Employees.FirstName%TYPE,
 vLastName IN Employees.LastName%TYPE,
 vReportsTo IN Employees.ReportsTo%TYPE
)
AS
BEGIN
	INSERT INTO Employees (EmployeeID, FirstName, LastName, ReportsTo)
	 VALUES (vEmployeeID, vFirstName, vLastName, vReportsTo);
END;
/

