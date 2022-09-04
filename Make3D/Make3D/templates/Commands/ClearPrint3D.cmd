@echo off
set usb="Unknown"
for /f %%D in ('wmic volume get DriveLetter^, Label ^| find /I "%BarnacleSDCard%"') do set usb=%%D
if %usb% equ "Unknown" goto nocard
@echo on
del "%usb%\%BarnacleProject%\*.gcode"
del "%usb%\%BarnacleProject%\*.ch*"
@echo off
exit

:nocard
Echo "Can't find card called %BarnacleSDCard%"
pause