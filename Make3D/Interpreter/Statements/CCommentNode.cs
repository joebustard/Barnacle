using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;


namespace ScriptLanguage
{
    class CCommentNode : StatementNode
    {
        private String _Text;
        public String Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

#region ctors
        // Instance constructor
        public CCommentNode()
        {
            Text = "";
        }
        // Copy constructor
        public CCommentNode( CCommentNode it )
        {
        }
#endregion
           /// Execute this node
        /// returning false terminates the application
        ///
         public override bool Execute()
        {
            return true;
        }

       /// Returns a String representation of this node that can be used for 
       /// Pretty Printing
       ///
       ///
        public override String ToRichText()
       {
          String result = Indentor.Indentation() + RichTextFormatter.LineComment("// "+Text);
          if (HighLight)
          {
              result = RichTextFormatter.Highlight(result);
          }
          return result;
       }

        public override String ToString()
        {
            String result = Indentor.Indentation() +"// " + Text;
           
            return result;
        }



    }
}


