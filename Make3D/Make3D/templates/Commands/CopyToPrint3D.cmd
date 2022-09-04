@echo off
set usb="Unknown"
for /f %%D in ('wmic volume get DriveLetter^, Label ^| find /I "%BarnacleSDCard%"') do set usb=%%D
if %usb% equ "Unknown" goto nocard
@echo on
mkdir "%usb%\%BarnacleProject%"
copy /Y  "%~dp0\..\printer\*.*" "%usb%\%BarnacleProject%\."
@echo off
pause
exit

:nocard
Echo "Can't find card called %BarnacleSDCard%"
pause