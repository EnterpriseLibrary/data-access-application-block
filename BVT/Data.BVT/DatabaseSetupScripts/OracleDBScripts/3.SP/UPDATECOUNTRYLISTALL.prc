CREATE OR REPLACE PROCEDURE UPDATECOUNTRYLISTALL
(vCountryCode IN Country.CountryCode%TYPE,
 vCountryName IN Country.CountryName%TYPE,
 cur_OUT OUT PKGENTLIB_ARCHITECTURE.CURENTLIB_ARCHITECTURE
)
AS
BEGIN
    UPDATE Country SET CountryName = vCountryName where CountryCode=vCountryCode;   
	OPEN cur_OUT for
	SELECT * FROM Country;
END;
/

