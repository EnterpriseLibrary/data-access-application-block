
    CREATE TABLE REGION
    (
        REGIONID            INTEGER             NOT NULL,
        REGIONDESCRIPTION   VARCHAR2 (50 BYTE)  NOT NULL 
    )
    TABLESPACE USERS
    PCTUSED    0
    PCTFREE    10
    INITRANS   1
    MAXTRANS   255
    STORAGE    (
                INITIAL          64K
                MINEXTENTS       1
                MAXEXTENTS       2147483645
                PCTINCREASE      0
                BUFFER_POOL      DEFAULT
               );


    CREATE UNIQUE INDEX REGION_PK ON REGION
    (REGIONID)
    TABLESPACE USERS
    PCTFREE    10
    INITRANS   2
    MAXTRANS   255
    STORAGE    (
                INITIAL          64K
                MINEXTENTS       1
                MAXEXTENTS       2147483645
                PCTINCREASE      0
                BUFFER_POOL      DEFAULT
               );


    ALTER TABLE REGION ADD (
      CONSTRAINT REGION_PK
     PRIMARY KEY
     (REGIONID)
        USING INDEX 
        TABLESPACE USERS
        PCTFREE    10
        INITRANS   2
        MAXTRANS   255
        STORAGE    (
                    INITIAL          64K
                    MINEXTENTS       1
                    MAXEXTENTS       2147483645
                    PCTINCREASE      0
                   ));
/

