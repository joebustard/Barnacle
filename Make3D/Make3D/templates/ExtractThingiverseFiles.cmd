mkdir c:\tmp\StlFiles\files
del /q "c:\tmp\StlFiles\files\*.stl"
set dnl=%userprofile%\downloads
mkdir %userprofile%\downloads\tmp
del /q %userprofile%\downloads\tmp\*.*

for %%f in (%dnl%\*.zip) do (
PowerShell Expand-Archive -Path "%%f" -DestinationPath "%userprofile%\downloads\tmp"
xcopy "%userprofile%\downloads\tmp\*.stl" "c:\tmp\stlfiles" /s
del /q "%userprofile%\downloads\tmp\*.*"
del /q "%userprofile%\downloads\tmp\files\*.*"
del /q "%userprofile%\downloads\tmp\images\*.*"
FOR /D %%p IN ("%userprofile%\downloads\tmp\*.*") DO rmdir "%%p" /s /q
)