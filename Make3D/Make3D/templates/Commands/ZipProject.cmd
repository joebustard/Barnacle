setlocal EnableDelayedExpansion

  rem make sure we know where 7zip is 
  set zipper="C:\Program Files\7-zip\7z.exe"
  if Exist %Zipper% (
  echo found zipper
  set src="%BarnacleFolder%"
  set bkp="%BarnacleFolder%\Backup"

  set trg="%BarnacleFolder%\Backup\%BarnacleProject%.zip"

  cd !bkp!

echo trg=!trg!
  rem back up any existing zip files ( up to 10)
  rem crude but works

  del /Q !BarnacleProject!_10.zip
  ren !BarnacleProject!_9.zip !BarnacleProject!_10.zip
  ren !BarnacleProject!_8.zip !BarnacleProject!_9.zip
  ren !BarnacleProject!_7.zip !BarnacleProject!_8.zip
  ren !BarnacleProject!_6.zip !BarnacleProject!_7.zip
  ren !BarnacleProject!_5.zip !BarnacleProject!_6.zip
  ren !BarnacleProject!_4.zip !BarnacleProject!_5.zip
  ren !BarnacleProject!_3.zip !BarnacleProject!_4.zip
  ren !BarnacleProject!_2.zip !BarnacleProject!_3.zip
  ren !BarnacleProject!_1.zip !BarnacleProject!_2.zip
  ren !BarnacleProject!.zip !BarnacleProject!_1.zip

  rem actually do the zip
  cd !src!
  %zipper% a !trg! *.bmf
  %zipper% a !trg! *.txt -r
  %zipper% a !trg! *.lmp -r
  %zipper% a !trg! *.cmd -r
  %zipper% a !trg! *.docx -r
  %zipper% a !trg! *.xlsx -r
 ) else (
 echo "Cant find 7zip, make sure  its installed."
 )

