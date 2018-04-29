CREATE OR REPLACE PROCEDURE ADDCOUNTRY
(vCountryCode IN Country.CountryCode%TYPE,
 vCountryName IN Country.CountryName%TYPE
)
AS
BEGIN
	INSERT INTO Country (CountryCode,CountryName)
	VALUES (vCountryCode,vCountryName);
END;
/

