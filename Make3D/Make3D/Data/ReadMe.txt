﻿v1.0.9
===========
Added
  Added top/bottom camera buttons to objectviewer user control.
  Five more primitives, staircase, squiggle, cross block, right bracket and screw diver bit
  Two "Wedge" profiles added to path loft
  Cross grille with edge
  Tray with straight sides tool
  Setting for the minimum number of faces to be added for every primitive.
  First cut of arc support in flexipath control
  Key strokes + and - to scale selected object by percentage given in object properties window
  Show number of vertices and faces on object properties page
  CSG function ForceUnion(Solid1,solid2) added to Limpet
  Horizontal Plane hcut statement added to Limpet
  Vertical Plane Cut added to editor
  GroupToMesh added to Limpet
  MakeCrossGrille to Limpet
  MirrorFront, MirrorBack, MirrorLeft, MirrorRight,MirrorUp,MirrorDown added to limpet
  Setlength(), setheight() and setwidth() to the standard limpet library
  Initial sdf modeller tool
  Allow user to explicitly set the x,y position of a point on a flexipath by typing in its position.
  Added extra colour definitions to the stanard limpet library
  Added circular paste funtion to the limpet  library
  Added SaveSolids statement to Limpet
  Added Flipdistal, fliphorizontal and flipvertical to limpet
  

Fixed
  Slice was showing estimated time wrong if it was more than a day
  Index()  function not working

Changed
  Platelet width from int to double to allow fractions of millimetres
  Changed key stroke for closing two selected objects from = to \
  Replaced Spacetree in CutH tool by Octtree to deal with large vertex counts better 
  Split the import/export tab on the project settings into two seperate ones
  Improved the curve interpolation of the aircraft rib fuselage tool
  Changed Min box side thicknesses to 0.1mm ( from 1mm)

v1.0.8
===========
Added
F12 key to record current camera position, shift F12 to restore it.
Automatically add seam to object when bending or folding it
Specify an angle between 1 and 90 for bend and fold operations. (Was previously a fixed angle of 10 degrees)
Reset parameter button on some tool dialogs that where missing.
Defer regeneration of solid on some longer operations if the user is still editing
Disable "save presets" on flexi path control if the same preset already exists
Added width to squirkle

Fixed
Bicorn tool was not saving and restoring editor parameters.
Squirkle tool was not saving and restoring editor parameters
Plankwall tool was not setting the origin to the center of the object.
Beziersurface was not applying presets correctly
Tidied preset wing shapes a little

Changed
Explicitly set size of main 3dviewport when mainwindow  size changes.
Explicitly set near clip plane distance for the camera
Refactored 3d viewport on tool dialogs to use a common control
Some code refactoring to reduce duplication
Convert some more tools to asynchronous generation

v1.0.7
===========
Added
  Autosave changes if idle for 5 minutes. Turn on and off from settings.
  Busy indicator on Image Plaque and Dual Profile
  Turn mm lines on floor grid on and off
  InsertPart() function to Limpet interpreter.
  Boxframe primitive.
  U-Beam primitive
  Shallow U-Beam primitive
  Two more basic textures for platelets
  Shift-Home key for back camera
  Insert for left camera
  Page Up for right camera
  U-Beam profile for path loft
  Slice individual parts in current file into separate gcode files

Fixed
  Renaming the current file, switching projects, then reopening project via the recent projects menu
  might lead to the last edited file not actually opening automatically
  RailWheel editor was mis-remembering the main radius setting.
  Improved memory recovery after running limpet script
  Prevent symbol corruption in the interpreter when script is re-run repeatedly
  ImagePlaque handled pixels with RGB < 128 and > 128 but 
  ignored any that are exactly 128. Fixed
  Prevent double undo checkpoint when rotating via 
  keyboard shortcuts.
  Export project was always doing a "floor all" even if this 
  option was turned off in the project settings

Changed
	Removed scrollviewer control from default view as it was
	causing issues on the script sub view
	Changed the header on the project explorer from "Solution" to "Project" as
	this makes more sense for most users.
	Prevent referenced groups from being ungrouped as this breaks the reference
	Prevent referenced objects from being grouped as this breaks the reference
	When loading referenced groups, the lower level objects are skipped to save memory
	Changed meaning of parameters in rail wheel tool.
	Show original names of objects on csg name confirmation dialog.
	Swapped panels on script view. Looks better.
	Replaced the code used to split objects in half. Much, much faster

v1.0.6
===========
Added
	Browse the textures folder from  the platelet dialog
	Barrel tool
	Oblique End Cylinder tool
	Added ability to lock aspect ratio when changing an objects size
	from the object properties panel.
	Added quick and dirty Union. Does not necessarily make a manifold union but is good enough
	for most models.
	Added Copy printer profile button to slice dialog
	Added Control S key to do a file save.
	Added Bevel options to brick wall tool.
	Added Bevel options to plank wall tool.
	Flexipathcontrol, split line segment can split quadbezier segment
	FlexipathControl, "Show Points" button, now toggles the visibilty rather
	than just temporarily showing points until the next click.
	Tooltips for controls on slice dialog
	Added another hole fixing method

Fixed
	Slice dialog not showing carriage returns in start/end gcode. Fixed
	Bug were canvas wing did not generate wing tip until parameters and changed back
	Platelet box option was using size to set the inside dimensions of the box
	rather than the outside. So platelet appeared to change size when user toggled between options

Changed
	Replaced subdivision method by a simpler one
	Dual tool sdf method. Also added three different sdf resolutions
	Allow adding sub folders to printer folder ( in case you have more than one printer!)

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
	Basic dimensioning Ctrl +right click on two seperate object points to display the distance between them. Press + key to close distance to 0
	Detect if a named memorycard is inserted while the slicing window is open and enable the copy to sd card button
	Delete part from the parts library
	First cut of a morphable object tool. i.e. Make a shape thats 50% sphere and 50% pyramid etc
	Show the top of the build volume for the selected printer (in addition to the existing floor marker)
	Backup menu option on File Menu. If the project has a Backup folder, then the main project files are zipped into Backup/<projname>_<date>_<time>.zip
	First cut of a bezier surface tool
	"Reset to defaults" added to ring lofting
	Mirror up/down
	Some presets for aircraft wing shapes.
	Allow import multi STLs dialog to specify a subfolder to create
	Allow path loft to use flat, round or square profiles

Fixed
	Prevent preset names being loaded twice in flexipathcontrol 
	Somehow lost ability to rename file in solution. Fixed.
	Stop shaped Wing using general platelet preset path strings
	Prevent exception if slicer log is empty.

Changed
	Make "Paste At" preserve the relative positions if there are multiple objects in the clipboard
	Make "Paste" offer to replace object if a single one is selected as the target
	Make Circular Paste and Multi Paste operate at the floor marker if "Place new obects at marker" setting selected
	Make Symbol use a simpler/faster algorithm and scale 
	Delete key now does delete, not cut
	Deleted the commands folder from the templates.
	Switch from CuraEngine 4 to CuraEngine 5 for integrated slicing (This still needs improving)
    Default slice Z Seam type to random
	Allow Vase loft to use preset path strings. Reduced the height of the default vase loft profile
	Tie the error reporting of unknown primitive types in the Limpet function Make() directly to the primitive factory.
	Replaced the pathloft algorithm completely from sdf to bufferedpolyline. Improves speed of path lofting dramatically

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