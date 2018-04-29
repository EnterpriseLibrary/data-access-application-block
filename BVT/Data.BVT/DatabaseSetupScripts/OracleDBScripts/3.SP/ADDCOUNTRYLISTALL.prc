CREATE OR REPLACE PROCEDURE ADDCOUNTRYLISTALL
(vCountryCode IN Country.CountryCode%TYPE,
 vCountryName IN Country.CountryName%TYPE,
 cur_OUT OUT PKGENTLIB_ARCHITECTURE.CURENTLIB_ARCHITECTURE
)
AS
BEGIN
	INSERT INTO Country (CountryCode,CountryName)
	VALUES (vCountryCode,vCountryName);
	OPEN cur_OUT for
	SELECT * FROM Country;
END;
/

