REM PARAMS taskid & revision number
::TaskId:[CompileTask | UnitTests | Pack] 

SET CI_FRAMEWORK=4.0
SET CI_Configuration=Debug
.\ReleaseRunner.cmd %1 %2 %3
