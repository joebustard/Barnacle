@echo off
set usb="Unknown"
for /f %%D in ('wmic volume get DriveLetter^, Label ^| find "PRINT3D"') do set usb=%%D
if %usb% equ "Unknown" goto nocard
@echo on
copy /Y  "%~dp0\..\printer\*.*" "%usb%\."
@echo off
pause
exit

:nocard
Echo "Can't find card called PRINT3D"
pause