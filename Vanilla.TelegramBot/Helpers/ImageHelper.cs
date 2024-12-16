using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Helpers
{
    public static class ImageHelper
    {
        public static async Task DownloadProfileImages(List<ImageModel> images)
        {
            foreach (var image in images)
            {
                Directory.CreateDirectory("storage");

                //var imagePath = "storage\\" + image.TgMediaId + ".jpg";
                var imagePath = Path.Combine("storage", image.TgMediaId + ".jpg");
                await DownloadImageAsync(image.DownloadPath, imagePath);
                MakeResizeImage(imagePath);
            }
        }

        static async Task DownloadImageAsync(string imageUrl, string fileName)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(imageUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fileName, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        static async Task MakeResizeImage(string path)
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
        }
    }
}
