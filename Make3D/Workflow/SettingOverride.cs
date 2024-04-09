namespace Workflow
{
    public class SettingOverride
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public SettingOverride(string s, string k, string v, string d)
        {
            Section = s;
            Key = k;
            Value = v;
            Description = d;
        }
    }
}