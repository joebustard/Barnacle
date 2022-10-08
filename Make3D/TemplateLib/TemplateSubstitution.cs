using System;

namespace TemplateLib
{
    internal class TemplateSubstitution
    {
        public string Original { get; set; }
        public string Replacement { get; set; }

        public TemplateSubstitution()
        {
            Original = String.Empty;
            Replacement = String.Empty;
        }
    }
}