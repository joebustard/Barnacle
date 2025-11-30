   // The actual scale of the  plans that the measurements are being taken from
   Double drawingscale = 31.9251336898396 ;

   // the scale we want the model to be at
   Double targetscale = 76 ;

   // is the model intended for filament or resin
   bool filament = true ;
   
   // The factor needed to resize the drawing dimensions to the target scale
   Double scalefactor = drawingscale / targetscale ;

