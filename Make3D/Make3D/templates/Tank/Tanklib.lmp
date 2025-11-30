

   function double ToPos( double v )
   {
	double res;
	res =v * scalefactor;
	return res;
   }
  
   function double ToScale( double v )
   {
	double res;
	double sign = sgn(v);
	res =v * scalefactor;
	if ( minimumDimension> 0 && Abs(res) < minimumDimension)
	{
	    res = sign * minimumDimension;
	}	
	return res;
   }

  Function Solid Addnutsalongx( Int  numnuts, Solid  nut, Double  sx, Double  sy, Double  sz, Double  dx, Double  dy, Solid  target, Double  ox )
   {
       Double nx ;
       Double ny ;
       Double ni ;
       Solid nn ;
       Move nut, Topos( sx) , Topos( sy) , Topos( sz)  ;
       For ni = 0 To  numnuts 
       {
           nx = ( ni * dx ) ;
           ny = ( ni * dy ) ;
           nn = Copy ( nut ) ;
           RMove nn, Topos( nx) , Topos( ny) , 0 ;
           If ( ox < 0 ) 
           {
               DropFront target, nn ;
           }
           Else 
           {
               DropBack target, nn ;
           }

           RMove nn, 0, 0, ox * Width ( nut ) / 2 ;
           target = Forceunion ( target, nn ) ;
       }
       Return target ;
   }

   Function Solid Addnutsalongz( Int  numnuts, Solid  nut, Double  sx, Double  sy, Double  sz, Double  dz, Double  dy, Solid  target, Double  oz )
   {
       Double nz ;
       Double ny ;
       Double ni ;
       Solid nn ;
       Move nut, Topos( sx) , Topos( sy) , Topos( sz)  ;
       For ni = 0 To  numnuts 
       {
           nz = ( ni * dz ) ;
           ny = ( ni * dy ) ;
           nn = Copy ( nut ) ;
           RMove nn, 0, Topos( ny) , Topos( nz)  ;
           If ( oz < 0 ) 
           {
               DropRight target, nn ;
           }
           Else 
           {
               DropLeft target, nn ;
           }

           RMove nn, oz * Length ( nut ) / 2, 0, 0 ;
           target = Forceunion ( target, nn ) ;
       }
       Return target ;
   }



