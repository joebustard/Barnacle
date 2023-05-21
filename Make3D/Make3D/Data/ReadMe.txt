v1.0.4
===========
Fixed
  Small speed up in hole fixing.

Changed
   Fonts on slicer dialog results

Added
   Path Loft tool
   Hollow Box Tool

v1.0.3
===========
Fixed
  Slicing dialog background not using style resource
  Catch exception raised when opening external doc without default app associated.
  Small intermittent bug where platlet would not save path in editor parameters
  Intermittent fault where image would not change when different library objects added

Changed
  Disable slice controls while copying to SD card
  Sort texture names in platelet tool
  Default texture depth changed from 1.0 to 0.5 
  Default Bill of Materials is now an xlsx not a docx. Columns match output from slice  
  Insert File/Reference File/Open FIle/Save As initial directory set to project folder
  Moved the 4 paste buttons under a ribbonmenubtton to save screen space.
  Replaced separate duplicate/unreferenced vertice fixer by a single one that does both and is faster

Added 
  Pill tool
  Roof Ridge Tool
  Simple Pie Slice Tool
  Textured disc/cyclinders

  Folder for user platelet textures. C:\programdata\barnacle\textures
  options to fit texture or tile across platelet
  Clone button. Makes duplicate of object in exactly the same place
  Ctrl-K key for clone
  Added F7/F8 key as short cuts for up/down Camera rotation  


  Slicer output shows number of objects in each file, estimated print time and estimated material.
      This is just the estimates reported by curaengine

v1.0.2
===========
Added Shaped Tiled Roof Tool
Added a project template for a model building
Added Home key as short cut for Home Camera
Added F5/F6 key as short cuts for left/right Camera rotation
Added some texture processing to platelet tool (still has some issues with holes)
Added Spring tool, which makes springs
Added automatic creation of empty library
Added ability to copy selected object from project to the library
added 2.5 and 25 mm to flexipath grid settings

Fixed bug where platelet tool "forgot" that a previously created plate was hollow
Fixed bug in Shaped Wall tool which prevented triangular walls
Fixed bug where lofted bezier ring did not remember editorparameters

Changed textbox on slice dialog to autoscroll to bottom as each file is sliced
Made flexipath control open the image selection dialog in the project Images folder

Changed multi STL import to use a dialog that allows rotations around the axies,
    Added options to import from a folder (as before) or from a zip
	better manages problems if the STLs cause out of memory exceptions
	Added a progress bar.
	Removed ExtractThingiverseFiles.cmd as program can now access a zip directly

v1.0.1
===========
Added simple Plank Wall Tool.
   
Platelet Tool
   Replaced by one with common flexipath control
   Added polar grid
   Corrected issues with hollow shapes

Linear Loft Tool
   Replaced by VaseLoft with common flexipath control
   Gives greater control over the shape of turned objects.

Added Shaped Brick Wall Tool
   Allows the user to draw an outline using the common flexipath tool
   and creates a brick wall

V1.0.0
===========
Initial Released Version