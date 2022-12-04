@echo off
title Switch branch - Genshin Impact Mod Pack 2023
chcp 65001 > nul
echo.⠀   ⠀⠀⠀⠀⠀⠀⠀⠀⢀⣤⡶⢶⣦⡀
echo.⠀  ⠀⠀⣴⡿⠟⠷⠆⣠⠋⠀⠀⠀⢸⣿
echo.⠀   ⠀⣿⡄⠀⠀⠀⠈⠀⠀⠀⠀⣾⡿                           Genshin Impact ReShade 2023 Mod Pack
echo.   ⠀⠀⠹⣿⣦⡀⠀⠀⠀⠀⢀⣾⣿                                       Switch branch
echo.⠀   ⠀⠀⠈⠻⣿⣷⣦⣀⣠⣾⡿
echo.    ⠀⠀⠀⠀⠀⠉⠻⢿⡿⠟
echo. ⠀   ⠀⠀⠀⠀⠀⠀⡟⠀⠀⠀⢠⠏⡆⠀⠀⠀⠀⠀⢀⣀⣤⣤⣤⣀⡀
echo. ⠀   ⠀⠀⡟⢦⡀⠇⠀⠀⣀⠞⠀⠀⠘⡀⢀⡠⠚⣉⠤⠂⠀⠀⠀⠈⠙⢦⡀
echo.   ⠀⠀⠀⠀⡇⠀⠉⠒⠊⠁⠀⠀⠀⠀⠀⠘⢧⠔⣉⠤⠒⠒⠉⠉⠀⠀⠀⠀⠹⣆      * Mod version: v3.0.0 [SV_DEV_04122022_300X-001]
echo.    ⠀⠀⠀⢰⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⠀⠀⣤⠶⠶⢶⡄⠀⠀⠀⠀⢹⡆    * ReShade version: v5.5.1
echo.   ⣀⠤⠒⠒⢺⠒⠀⠀⠀⠀⠀⠀⠀⠀⠤⠊⠀⢸⠀⡿⠀⡀⠀⣀⡟⠀⠀⠀⠀⢸⡇     * FPS unlocker version: v2.0.5
echo.  ⠈⠀⠀⣠⠴⠚⢯⡀⠐⠒⠚⠉⠀⢶⠂⠀⣀⠜⠀⢿⡀⠉⠚⠉⠀⠀⠀⠀⣠⠟
echo.   ⠠⠊⠀⠀⠀⠀⠙⠂⣴⠒⠒⣲⢔⠉⠉⣹⣞⣉⣈⠿⢦⣀⣀⣀⣠⡴⠟
echo ========================================================================================= & echo.


echo 1/6 - Checking if git is installed...
if exist "C:\Program Files\Git\cmd\git.exe" (
    git -v
) else (
    echo Not installed! I can't check for new updates.
    echo Please download: https://git-scm.com/downloads
    goto nothing_to_do
)
echo.


echo 2/6 - Killing required processes...
taskkill /IM "Genshin Impact Mod Pack.exe"
taskkill /IM "inject.exe"
taskkill /IM "unlockfps_clr.exe"
echo.


echo 3/6 - git branch --show-current
set branch=
for /F "delims=" %%n in ('git branch --show-current') do set "branch=%%n"
if "%branch%"=="" echo Not a git branch. && goto :EOF
echo %branch%
echo.

if "%branch%" == "main" (
    echo 4/6 - Git checkout dev
    git checkout dev
) else if "%branch%" == "dev" (
    echo 4/6 - Git checkout main
    git checkout main
) else (
    echo 4/6 - Git checkout
    echo Error: Unknown branch.
    goto nothing_to_do
)
echo.


echo 5/6 - Changing branch...
if "%branch%" == "main" (
    git pull . origin/dev
    echo. && echo.
    echo 🎉 You are in dev branch. !! BETA RELEASES !!
) else if "%branch%" == "dev" (
    git pull . origin/main
    echo. && echo.
    echo 🎉 You are in main branch.
) else (
    echo Unknown branch.
    goto nothing_to_do
)
echo.


echo 6/6 - Relaunching...
echo You can close this window ฅ^˙Ⱉ˙^ฅ rawr!
"Genshin Impact Mod Pack.exe"

:nothing_to_do
set /p 0=
exit