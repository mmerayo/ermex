REM PARAMS taskid & revision number
::TaskId:[Compile | UnitTests | Pack] 

SET CI_CPU=x86
SET CI_FRAMEWORK=4.0
SET CI_Configuration=Debug
.\ReleaseRunner.cmd %1 %2
