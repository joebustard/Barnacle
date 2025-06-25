using System;

namespace TemplateLib
{
    public class TemplateSubstitution
    {
        public TemplateSubstitution()
        {
            Original = String.Empty;
            Replacement = String.Empty;
        }

        public string Original
        {
            get; set;
        }

        public string Replacement
        {
            get; set;
        }
    }
}