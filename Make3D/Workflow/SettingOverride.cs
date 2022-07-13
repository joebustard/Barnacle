namespace Workflow
{
    public class SettingOverride
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public SettingOverride(string k, string v)
        {
            Key = k;
            Value = v;
        }
    }
}