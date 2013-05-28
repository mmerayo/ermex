
BEGIN TRANSACTION;
CREATE TEMPORARY TABLE Components_backup(
    Components_Id                   INTEGER PRIMARY KEY AUTOINCREMENT
                                            NOT NULL
                                            UNIQUE,
    Components_ComponentId          GUID    NOT NULL,
    Components_ComponentOwner       GUID    NOT NULL,
    Components_Version              INTEGER NOT NULL,
    Components_Latency              INTEGER NOT NULL,);

INSERT INTO Components_backup SELECT Components_Id,Components_ComponentId,Components_ComponentOwner,Components_Version,Components_Latency FROM Components;
DROP TABLE Components;
CREATE TABLE Components (
	Components_Id                   INTEGER PRIMARY KEY AUTOINCREMENT 
											NOT NULL
                                            UNIQUE,
    Components_ComponentId          GUID    NOT NULL,
    Components_ComponentOwner       GUID    NOT NULL,
    Components_Version              INTEGER NOT NULL,
    Components_Latency              INTEGER NOT NULL,);
INSERT INTO Components SELECT Components_Id,Components_ComponentId,Components_ComponentOwner,Components_Version,Components_Latency FROM Components_backup;
DROP TABLE Components_backup;
COMMIT;
GO

