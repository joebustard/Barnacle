using System;

namespace Barnacle.Dialogs.Slice
{
    public class ProfileEntry
    {
        public String SettingName { get; set; }
        public String SettingValue { get; set; }
        public String SettingDescription { get; set; }

        public ProfileEntry(string n, string v, string d)
        {
            SettingName = n;
            SettingValue = v;
            SettingValue = d;
        }
    }
}