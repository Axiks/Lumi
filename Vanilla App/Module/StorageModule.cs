namespace Vanilla_App.Module
{
    public class StorageModule
    {
        const string STORAGE_DIRECTORY_NAME = "storage";

        public async Task<string> DownloadFile(DownloadFileRequestModel fileRequest)
        {
            var curentEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            string _pathToFiles = "";
            switch (curentEnvironment)
            {
                case "Development":
                    // _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), "Vanilla.Common", _configFileName);
                    // if(Environment.GetEnvironmentVariable("DOTNET_DASHBOARD_OTLP_ENDPOINT_URL") is not null) _pathToSettingFile = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), "Vanilla.Common", _configFileName); // fix
                    _pathToFiles = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..")), STORAGE_DIRECTORY_NAME);
                    if (AppDomain.CurrentDomain.FriendlyName == "Vanilla.Aspire.ApiService") _pathToFiles = Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..")), STORAGE_DIRECTORY_NAME); // fix

                    break;
                case "Docker-Production":
                    _pathToFiles = Path.Combine(System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), STORAGE_DIRECTORY_NAME);
                    break;
                default:
                    Console.WriteLine("environment not recognized");
                    return "";
            }

            var directoryInfo = Directory.CreateDirectory(_pathToFiles);
            var fileName = fileRequest.FileName ?? Guid.NewGuid().ToString();
            fileName += ".";
            fileName += fileRequest.DownloadURL.Split(".").Last();

            var filePath = Path.Combine(_pathToFiles, fileName);
            await DownloadFileAsync(fileRequest.DownloadURL, filePath);
            //await MakeResizeImage(imagePath);

            return fileName;
        }

        static async Task DownloadFileAsync(string fileUrl, string fileName)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(fileUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fileName, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        public async Task<bool> RemoveFile(string fileName)
        {
            var filePath = Path.Combine(STORAGE_DIRECTORY_NAME, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        /*      static async Task MakeResizeImage(string path)
              {
                  var original = System.Drawing.Image.FromFile(path);
                  var thumbnail = await ScaleImage(original, 256);

                  var iamgePathWichoutJpg = path.Split(".jpg").First();
                  var thumbnailPath = iamgePathWichoutJpg + "_thumbnail.jpg";
                  thumbnail.Save(thumbnailPath);
              }

              static async Task<System.Drawing.Image> ScaleImage(System.Drawing.Image image, int height)
              {
                  double ratio = (double)height / image.Height;
                  int newWidth = (int)(image.Width * ratio);
                  int newHeight = (int)(image.Height * ratio);
                  Bitmap newImage = new Bitmap(newWidth, newHeight);
                  using (Graphics g = Graphics.FromImage(newImage))
                  {
                      g.DrawImage(image, 0, 0, newWidth, newHeight);
                  }
                  image.Dispose();
                  return newImage;
              }*/
    }
}
