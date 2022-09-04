@echo off
set usb="Unknown"
for /f %%D in ('wmic volume get DriveLetter^, Label ^| find /I "%BarnacleSDCard%"') do set usb=%%D
if %usb% equ "Unknown" goto nocard
@echo on
del "%usb%\%BarnacleProject%\*.gcode"
del "%usb%\%BarnacleProject%\*.ch*"

@echo searching for latest file
FOR /F "eol=| delims=" %%I IN ('DIR "%~dp0\..\printer\*.*" /A-D /B /O-D /TW 2^>nul') DO (
    SET NewestFile=%%I
    GOTO FoundFile
)
ECHO No printer file file found!
pause
exit

:FoundFile
ECHO Newest *.gcode file is: %NewestFile%
copy /Y  %NewestFile% "%usb%\%BarnacleProject%\."
@echo off
pause
exit

:nocard
Echo "Can't find card called %BarnacleSDCard%"
pause