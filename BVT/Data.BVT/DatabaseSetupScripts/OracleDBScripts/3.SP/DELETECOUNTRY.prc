CREATE OR REPLACE PROCEDURE DELETECOUNTRY
(vCountryCode IN Country.CountryCode%TYPE
)
AS
BEGIN
    DELETE FROM Country where CountryCode=vCountryCode;   
END;
/

