
    CREATE TABLE ORDERS
    (
        ORDERID         INTEGER         NOT NULL,
        CUSTOMERID      VARCHAR2 (5 BYTE)   NULL,
	    EMPLOYEEID      INTEGER         NULL,
	    ORDERDATE       DATE            NULL,
	    REQUIREDDATE    DATE            NULL,
	    SHIPPEDDATE     DATE            NULL,
	    SHIPVIA         INTEGER         NULL,
	    FREIGHT         NUMBER          DEFAULT 0 NULL ,
	    SHIPNAME        NVARCHAR2 (80)  NULL,
	    SHIPADDRESS     NVARCHAR2 (120) NULL,
	    SHIPCITY        NVARCHAR2 (30)  NULL,
	    SHIPREGION      NVARCHAR2 (30)  NULL,
	    SHIPPOSTALCODE  NVARCHAR2 (20)  NULL,
	    SHIPCOUNTRY     NVARCHAR2 (30)  NULL,
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (CUSTOMERID) REFERENCES CUSTOMERS (CUSTOMERID),
	    CONSTRAINT FK_Orders_Employees FOREIGN KEY (EMPLOYEEID) REFERENCES EMPLOYEES (EMPLOYEEID)
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


    CREATE UNIQUE INDEX PK_ORDERID ON ORDERS
    (ORDERID)
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


    ALTER TABLE ORDERS ADD (
      CONSTRAINT PK_ORDERID
     PRIMARY KEY
     (ORDERID)
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