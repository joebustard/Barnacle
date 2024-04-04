namespace Workflow
{
    public class SettingOverride
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public SettingOverride(string k, string v, string d)
        {
            Key = k;
            Value = v;
            Description = d;
        }
    }
}