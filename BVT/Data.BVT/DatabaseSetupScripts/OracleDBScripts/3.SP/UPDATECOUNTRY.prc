CREATE OR REPLACE PROCEDURE UPDATECOUNTRY
(vCountryCode IN Country.CountryCode%TYPE,
 vCountryName IN Country.CountryName%TYPE
)
AS
BEGIN
    UPDATE Country SET CountryName = vCountryName where CountryCode=vCountryCode;   
END;
/

