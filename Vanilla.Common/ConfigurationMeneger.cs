using Microsoft.Extensions.Configuration;


namespace Vanilla.Common
{
    public class ConfigurationMeneger
    {
        string _configFileName = "appsettings.json";
        public SettingsModel? _settings;

        public ConfigurationMeneger()
        {
            //string _pathToSettingFile = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Vanilla.Common", "appsettings.json");
            //string _pathToSettingFile = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).ToString(), "Vanilla.Common", "appsettings.json");

            // !!! Unstable FIX !!!
            var curentEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");


            string _pathToSettingFile = "";
            switch (curentEnvironment) { 
                case "Development":
                    _configFileName = "appsettings.Development.json";
                    _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), "Vanilla.Common", _configFileName);
                    break;
                case "Docker-Production":
                    _configFileName = "appsettings.Docker-Production.json";
                    _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), _configFileName);
                    break;
                default:
                    Console.WriteLine("environment not recognized");
                    return;
            }

            //string _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), "Vanilla.Common", "appsettings.json");

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
