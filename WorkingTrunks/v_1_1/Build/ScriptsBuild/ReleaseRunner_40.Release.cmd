REM PARAMS taskid & revision number
::TaskId:[Compile | UnitTests | Pack] 

SET CI_FRAMEWORK=4.0
SET CI_Configuration=Release
.\ReleaseRunner.cmd %1 %2 %3