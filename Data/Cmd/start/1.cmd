echo 3/3 - Waiting for processes...
tasklist /fi "ImageName eq unlockfps_clr.exe" /fo csv 2>NUL | find /I "unlockfps_clr.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [✓] unlockfps_clr.exe - OK
) else (
    echo [x] unlockfps_clr.exe - ERROR
    call Data\Cmd\start\error.cmd
    pause
)

tasklist /fi "ImageName eq inject.exe" /fo csv 2>NUL | find /I "inject.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [✓] inject.exe - OK
    call Data\Cmd\start\done.cmd
) else (
    echo [x] inject.exe - ERROR
    call Data\Cmd\start\error.cmd
    pause
)