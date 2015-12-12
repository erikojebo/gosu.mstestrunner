namespace Gosu.MsTestRunner.UI.Infrastructure
{
    public class UserSettingsService
    {
        public string LastConfigFilePath
        {
            get
            {
                return Properties.Settings.Default.LastConfigFilePath;
            }
            set
            {
                Properties.Settings.Default.LastConfigFilePath = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}