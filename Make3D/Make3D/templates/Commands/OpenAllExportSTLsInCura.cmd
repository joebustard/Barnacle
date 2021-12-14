@echo off
set Cura="C:\Program Files\Ultimaker Cura 4.9.0\cura.exe"
set ExportDir=%BarnacleFolder%/Export
pushd "%ExportDir%"
for /r %%v in (*.stl) do %Cura% "%%v" 
popd


