using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.Slice
{
    public class ProfileEntry
    {
        public String SettingName { get; set; }
        public String SettingValue { get; set; }

        public ProfileEntry( string n, string v)
        {
            SettingName = n;
            SettingValue = v;
        }
    }
}
