Dim fso, sqlfile, f1, fc
Dim sFileName
Dim sysInfo
Dim i

Set fso = CreateObject("Scripting.FileSystemObject")
Set sysInfo = CreateObject( "ADSystemInfo" )


sFileName = CStr(DatePart("yyyy",Date)) 
sFileName = sFileName + Right("0" + Cstr(DatePart("m",Date)) , 2)
sFileName = sFileName + Right("0" + Cstr(DatePart("d",Date)) , 2)
'sFileName = sFileName + "_"
sFileName = sFileName + Right("0" + Cstr(DatePart("h",Time)) , 2)
sFileName = sFileName + Right("0" + Cstr(DatePart("n",Time)) , 2)
sFileName = sFileName + Right("0" + Cstr(DatePart("s",Time)) , 2)

sFileName = sFileName + ".sql"


sFileName = fso.GetParentFolderName("") + sFileName

fso.CreateTextFile(sFileName)

Set sqlfile= fso.OpenTextFile(sFileName, 8, True)

sqlfile.WriteLine("")
sqlfile.WriteLine("-- ===================================================================")
sqlfile.WriteLine("-- SQL Update Script (Related Changes)")
sqlfile.WriteLine("-- Populate these details for version management")
sqlfile.WriteLine("-- ===================================================================")
sqlfile.WriteLine("-- Author: " )
sqlfile.WriteLine("-- Description:")
sqlfile.WriteLine("-- Comment:")
sqlfile.WriteLine("-- Version:")
sqlfile.WriteLine("-- ===================================================================")
sqlfile.WriteLine("")

sqlfile.Close
Set sqlfile = Nothing
Set fso = Nothing
