CREATE OR REPLACE PROCEDURE GetEmployeeLastName(vName IN OUT Employees.FIRSTNAME%TYPE)
AS
BEGIN
    SELECT LastName INTO vName
    FROM Employees
    WHERE FirstName = vName;
END;
/ 