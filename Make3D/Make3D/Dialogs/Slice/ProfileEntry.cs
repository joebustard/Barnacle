using System;

namespace Barnacle.Dialogs.Slice
{
    public class ProfileEntry
    {
        public String SettingName { get; set; }
        public String SettingValue { get; set; }

        public ProfileEntry(string n, string v)
        {
            SettingName = n;
            SettingValue = v;
        }
    }
}