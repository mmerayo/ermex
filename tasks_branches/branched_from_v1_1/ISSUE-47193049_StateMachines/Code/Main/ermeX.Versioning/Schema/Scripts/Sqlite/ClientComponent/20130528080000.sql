CREATE TEMPORARY TABLE Components_backup(
    Components_Id                   INTEGER PRIMARY KEY AUTOINCREMENT
                                            NOT NULL
                                            UNIQUE,
    Components_ComponentId          GUID    NOT NULL,
    Components_ComponentOwner       GUID    NOT NULL,
    Components_Version              INTEGER NOT NULL,
    Components_Latency              INTEGER NOT NULL);

GO

INSERT INTO Components_backup SELECT Components_Id,Components_ComponentId,Components_ComponentOwner,Components_Version,Components_Latency FROM Components;

GO

DROP TABLE Components;

GO

CREATE TABLE Components (
	Components_Id                   INTEGER PRIMARY KEY AUTOINCREMENT 
											NOT NULL
                                            UNIQUE,
    Components_ComponentId          GUID    NOT NULL,
    Components_ComponentOwner       GUID    NOT NULL,
    Components_Version              INTEGER NOT NULL,
    Components_Latency              INTEGER NOT NULL);

GO

INSERT INTO Components SELECT Components_Id,Components_ComponentId,Components_ComponentOwner,Components_Version,Components_Latency FROM Components_backup;

GO

DROP TABLE Components_backup;

GO

