v1.0.5
===========
Added
	Basic hole support in platelet
	Option to prompt for new object name after group operation
	Round Grille Tool
	Ctrl+S up|down|left|right shortcut keys for stacking
	Ctrl+L up|down|left|right shortcut keys for alignment
	Different Print Distribution method. Creates a more compact bed layout for printing 
	An Octahedron primitive and spiky triangle primitive
	IBeam and XBeam primitives
	Ctrl-X for cut
	Ctrl-O to switch to object properties panel
	Name of initial file to open added to templates
	Show object type (i.e. primitive, group etc) in object properties window
	Cut all faces and vertices of object below a moveable horizontal plane
	Subdivide
	Basic dimensioning Ctrl +right click on two seperate object points to display the distance between them
	Detect if a named memorycard is inserted while the slicing window is open and enable the copy to sd card button
	Delete part from the parts library
	First cut of a morphable object tool. i.e. Make a shape thats 50% sphere and 50% pyramid etc
	Show the top of the build volume for the selected printer (in addition to the existing floor marker)
	Backup menu option on File Menu. If the project has a Backup folder, then the main project files are zipped into Backup/<projname>_<date>_<time>.zip

Fixed
	Prevent preset names being loaded twice in flexipathcontrol 
	Somehow lost ability to rename file in solution. Fixed.
	Stop shaped Wing using general platelet preset path strings
	Prevent exception if slicer log is empty.

Changed
	Make "Paste At" preserve the relative positions if there are multiple objects in the clipboard
	Make "Paste" offer to replace object if a single one is selected as the target
	Make Symbol use a simpler/faster algorithm and scale 
	Delete key now does delete, not cut
	Deleted the commands folder from the templates.
	Switch from CuraEngine 4 to CuraEngine 5 for integrated slicing (This still needs improving)
    Default slice Z Seam type to random
	Allow Vase loft to use preset path strings
	Tie the error reporting of unknown primitive types in the Limpet function Make() directly to the primitive factory.

v1.0.4
===========

Added
   Shaped aircraft wing tool. Similar to the basic wing and canvas wing tools but allows the outline of the wing to drawn so
   curved wings like those of a Spitfire can be created.

   All three wing tools now render an outline image of the wing profile

   Path Loft tool

   Simple Hollow Box Tool

   Box option on platelet.

   Rectangular grille tool

   Symbol tool to turn a character from a symbol font into a 3D object

   Simple construction strip tool. Makes a strip with holes as found in children's cnstruction sets.

   A couple of extra tyre shapes to the wheel.

   Mirror object option

   First cut of screen capture

Fixed
  Small speed up in hole fixing.

  Small speed up in smoothing. Still slow for objects with high vertice counts.

  Explore folder menu option wasn't working

  Copying flexipath string from one tool to another wasn't always resulting in objects updating

  Converting final segment of flexipath to a quadratic bezier always gave a cubic bezier instead. Fixed

  Exporting/slicing a file that comprised references to models in other files could cause rotation issues. Fixed 
  Text tool not updating combobox with the font name when text reopened. Fixed

Changed
   Fonts on slicer dialog results enlarged.

   Added presets list to flexipath editor. Some supplied in installation. User can create his own file.

   Added colour, thickness and opacity settings for rendering flexipath curves.

   Added split quad bezier into two quad beziers that pass through the same points

   Flexipath grid and line settings saved and restored. Shared across all tools that use a flexipath.

   Aircraft Profile Fuselage uses the same flexipath control as the other tools.
     Improved the fuselage model generation.
  
  Enable the rotation preset buttons even when no object selected
  
  Object properties palette changes size of selected object as text changes, not after focus lost



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