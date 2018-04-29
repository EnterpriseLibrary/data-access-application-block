create or replace 
PROCEDURE sp_GUIDTEST ( vGuidInput IN raw, vGuidOutput OUT raw)
	AS

BEGIN
	SELECT vGuidInput INTO vGuidOutput FROM DUAL;
END;
/

