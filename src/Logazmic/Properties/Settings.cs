namespace Logazmic.Properties
{
    internal partial class Settings
    {
        public Settings()
        {
            PropertyChanged += (sender, args) => Save();
        }
    }
}