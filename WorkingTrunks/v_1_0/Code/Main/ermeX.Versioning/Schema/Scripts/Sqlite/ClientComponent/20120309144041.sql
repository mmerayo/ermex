


CREATE TABLE Components ( 
    Components_Id                   INTEGER PRIMARY KEY AUTOINCREMENT
                                            NOT NULL
                                            UNIQUE,
    Components_ComponentId          GUID    NOT NULL,
    Components_ComponentOwner       GUID    NOT NULL,
    Components_Version              INTEGER NOT NULL,
    Components_Latency              INTEGER NOT NULL,
    Components_IsRunning            BOOLEAN     NOT NULL,
    Components_ExchangedDefinitions BOOLEAN     NOT NULL,
    Components_ComponentExchanges   GUID 
);

GO


CREATE UNIQUE INDEX Components_UQ__Componen__66FAE64429F972FF ON Components ( 
    Components_ComponentId    DESC,
    Components_ComponentOwner DESC 
);

GO


CREATE TABLE ConnectivityDetails ( 
    ConnectivityDetails_Id             INTEGER           PRIMARY KEY AUTOINCREMENT
                                                         NOT NULL
                                                         UNIQUE,
    ConnectivityDetails_Ip             NVARCHAR( 1024 )  NOT NULL
                                                         COLLATE 'NOCASE',
    ConnectivityDetails_Port           INTEGER           NOT NULL,
    ConnectivityDetails_IsLocal        BOOLEAN               NOT NULL,
    ConnectivityDetails_ComponentOwner GUID              NOT NULL,
    ConnectivityDetails_Version        INTEGER           NOT NULL,
    ConnectivityDetails_ServerId       GUID              NOT NULL 
);

GO



CREATE TABLE IncomingMessageSuscriptions ( 
    IncomingMessageSuscriptions_Id                     INTEGER           PRIMARY KEY AUTOINCREMENT
                                                                         NOT NULL
                                                                         UNIQUE,
    IncomingMessageSuscriptions_BizMessageFullTypeName NVARCHAR( 256 )   NOT NULL
                                                                         COLLATE 'NOCASE',
    IncomingMessageSuscriptions_DateLastUpdateUtc      INTEGER           NOT NULL,
    IncomingMessageSuscriptions_ComponentOwner         GUID              NOT NULL,
    IncomingMessageSuscriptions_Version                INTEGER           NOT NULL,
    IncomingMessageSuscriptions_SuscriptionHandlerId   GUID              NOT NULL,
    IncomingMessageSuscriptions_HandlerType            NVARCHAR( 1024 )  NOT NULL
                                                                         COLLATE 'NOCASE' 
);

GO

CREATE UNIQUE INDEX IncomingMessageSuscriptions_IX_IncomingMessageSuscriptions ON IncomingMessageSuscriptions ( 
    IncomingMessageSuscriptions_BizMessageFullTypeName DESC,
    IncomingMessageSuscriptions_SuscriptionHandlerId   DESC 
);

GO


CREATE TABLE IncomingMessages ( 
    IncomingMessages_Id                     INTEGER           PRIMARY KEY AUTOINCREMENT
                                                              NOT NULL
                                                              UNIQUE,
    IncomingMessages_PublishedBy            GUID              NOT NULL,
    IncomingMessages_PublishedTo            GUID              NOT NULL,
    IncomingMessages_CreatedTimeUtc       INTEGER           NOT NULL,
    IncomingMessages_TimeReceivedUtc        INTEGER           NOT NULL,
    IncomingMessages_ComponentOwner         GUID              NOT NULL,
    IncomingMessages_Version                INTEGER           NOT NULL,
    IncomingMessages_SuscriptionHandlerId   GUID              NOT NULL,
	IncomingMessages_MessageId      GUID     NOT NULL,    
    IncomingMessages_JsonMessage    NVARCHAR NOT NULL
                                        COLLATE 'NOCASE',
    IncomingMessages_Status        INTEGER  NOT NULL 
);

GO



CREATE TABLE OutgoingMessageSuscriptions ( 
    OutgoingMessageSuscriptions_Id                     INTEGER          PRIMARY KEY AUTOINCREMENT
                                                                        NOT NULL
                                                                        UNIQUE,
    OutgoingMessageSuscriptions_BizMessageFullTypeName NVARCHAR( 256 )  NOT NULL
                                                                        COLLATE 'NOCASE',
    OutgoingMessageSuscriptions_DateLastUpdateUtc      INTEGER          NOT NULL,
    OutgoingMessageSuscriptions_Version                INTEGER          NOT NULL,
    OutgoingMessageSuscriptions_ComponentId            GUID             NOT NULL,
    OutgoingMessageSuscriptions_ComponentOwner         GUID             NOT NULL 
);

GO


CREATE UNIQUE INDEX OutgoingMessageSuscriptions_IX_OutgoingMessageSuscriptions ON OutgoingMessageSuscriptions ( 
    OutgoingMessageSuscriptions_ComponentOwner         DESC,
    OutgoingMessageSuscriptions_ComponentId            DESC,
    OutgoingMessageSuscriptions_BizMessageFullTypeName DESC 
);

GO


CREATE TABLE OutgoingMessages ( 
    OutgoingMessages_Id                     INTEGER           PRIMARY KEY AUTOINCREMENT
                                                              NOT NULL
                                                              UNIQUE,
    OutgoingMessages_PublishedBy            GUID              NOT NULL,
    OutgoingMessages_PublishedTo            GUID              NOT NULL,
    OutgoingMessages_CreatedTimeUtc       INTEGER           NOT NULL,
    OutgoingMessages_Tries                  INTEGER           NOT NULL,
    OutgoingMessages_Failed                 BOOLEAN               NOT NULL,
    OutgoingMessages_Version                INTEGER           NOT NULL,
    OutgoingMessages_ComponentOwner         GUID              NOT NULL,
    OutgoingMessages_Delivering             BOOLEAN               NOT NULL,
	OutgoingMessages_MessageId      GUID     NOT NULL,
    OutgoingMessages_JsonMessage    NVARCHAR NOT NULL
                                        COLLATE 'NOCASE',
    OutgoingMessages_Status        INTEGER  NOT NULL  
);


GO



CREATE TABLE ServicesDetails ( 
    ServicesDetails_Id                              INTEGER           PRIMARY KEY AUTOINCREMENT
                                                                      NOT NULL
                                                                      UNIQUE,
    ServicesDetails_ServiceImplementationMethodName NVARCHAR( 1024 )  NOT NULL
                                                                      COLLATE 'NOCASE',
    ServicesDetails_ServiceImplementationTypeName   NVARCHAR( 1024 )  NOT NULL
                                                                      COLLATE 'NOCASE',
    ServicesDetails_ServiceInterfaceTypeName        NVARCHAR( 1024 )  NOT NULL
                                                                      COLLATE 'NOCASE',
    ServicesDetails_OperationIdentifier             GUID              NOT NULL,
    ServicesDetails_Publisher                       GUID              NOT NULL,
    ServicesDetails_Version                         INTEGER           NOT NULL,
    ServicesDetails_ComponentOwner                  GUID              NOT NULL,
    ServicesDetails_IsSystemService                 BOOLEAN               NOT NULL 
);
GO


CREATE UNIQUE INDEX ServicesDetails_idx_uniqueoperation ON ServicesDetails ( 
    ServicesDetails_OperationIdentifier DESC,
    ServicesDetails_Publisher           DESC,
    ServicesDetails_ComponentOwner      DESC 
);
GO



CREATE TABLE ChunkedServiceRequestMessages ( 
    ChunkedServiceRequestMessages_Id             INTEGER PRIMARY KEY AUTOINCREMENT
                                                        NOT NULL
                                                        UNIQUE,
    ChunkedServiceRequestMessages_Operation      GUID    NOT NULL,
    ChunkedServiceRequestMessages_Data           BLOB    NOT NULL,
    ChunkedServiceRequestMessages_CorrelationId  GUID    NOT NULL,
    ChunkedServiceRequestMessages_Order          INTEGER NOT NULL,
    ChunkedServiceRequestMessages_Eof            BOOLEAN     NOT NULL,
    ChunkedServiceRequestMessages_Version        INTEGER NOT NULL,
    ChunkedServiceRequestMessages_ComponentOwner GUID    NOT NULL 
);
GO

CREATE UNIQUE INDEX ChunkedServiceRequestMessages_IX_ChunkedServiceRequestMessages ON ChunkedServiceRequestMessages ( 
    ChunkedServiceRequestMessages_CorrelationId ASC,
    ChunkedServiceRequestMessages_Order         ASC,
	ChunkedServiceRequestMessages_ComponentOwner ASC 
);
GO

