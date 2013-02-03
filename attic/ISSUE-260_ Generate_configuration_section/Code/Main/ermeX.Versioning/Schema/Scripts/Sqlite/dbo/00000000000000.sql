CREATE TABLE IF NOT EXISTS Version ( 
    Version_Id          INTEGER        PRIMARY KEY AUTOINCREMENT
                                       NOT NULL,
    Version_TimeStamp   VARCHAR( 64 )  NOT NULL
                                       COLLATE NOCASE,
    Version_SchemaType  INTEGER        NOT NULL,
    Version_DateApplied DATETIME       NOT NULL 
);

GO
