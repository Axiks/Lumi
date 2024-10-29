using Microsoft.Extensions.Configuration;


namespace Vanilla.Common
{
    public class ConfigurationMeneger
    {
        private static readonly string _configFileName = "appsettings.json";
        public SettingsModel? _settings;

        public ConfigurationMeneger()
        {
            //string _pathToSettingFile = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Vanilla.Common", "appsettings.json");
            //string _pathToSettingFile = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).ToString(), "Vanilla.Common", "appsettings.json");

            // !!! Unstable FIX !!!
            string _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), "Vanilla.Common", "appsettings.json");


            var _config = new ConfigurationBuilder()
                .AddJsonFile(_pathToSettingFile)
                .AddEnvironmentVariables()
                .Build();
            _settings = _config.GetRequiredSection("Settings").Get<SettingsModel>();

            if (_settings == null) throw new Exception("No found setting section");
        }

        public SettingsModel? Settings => _settings;
    }
}
