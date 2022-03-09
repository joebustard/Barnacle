@echo off
set Cura="%BarnacleSlicer%"
set ExportDir=%BarnacleFolder%/Export
pushd "%ExportDir%"
for /r %%v in (*.stl) do %Cura% "%%v" 
popd


