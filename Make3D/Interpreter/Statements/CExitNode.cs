using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;


namespace ScriptLanguage
{
    class CExitNode : CStatementNode
    {
#region ctors
        // Instance constructor
        public CExitNode()
        {
        }
        // Copy constructor
        public CExitNode( CExitNode it )
        {
        }
#endregion
           /// Execute this node
        /// returning false terminates the application
        ///
         public override bool Execute()
        {
            //
            // returning false will terminate the application
            //
            return false;
        }

       /// Returns a String representation of this node that can be used for 
       /// Pretty Printing
       ///
       ///
        public override String ToRichText()
       {
          String result = Indentor.Indentation()+RichTextFormatter.KeyWord("Exit") +" ;";
          if (HighLight)
          {
              result = RichTextFormatter.Highlight(result);
          }
          return result;
       }

        public override String ToString()
        {
            String result = Indentor.Indentation() +"Exit ;" ;

            return result;
        }



    }
}


