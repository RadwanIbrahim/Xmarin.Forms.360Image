using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Image360
{
    public class ImageDownloadManager
    {
        private static ConcurrentDictionary<string, Task> Downloads = new ConcurrentDictionary<string, Task>();

        private bool IsImageExists(string url)
        {
            var localurl = GetLocalPath(url);
            if (File.Exists(localurl))
                return true;
            else
                return false;
        }

        private string GetLocalPath(string Url)
        {
            var fileName = Url.Split('/').Last();
            var appData = GetImagesFolderPath();
            var imagefilePath = Path.Combine(appData, fileName);
            return imagefilePath;

        }

        public async Task<string> GetLocalUrl(string Url, bool isCacheEnabled = true)
        {
            try
            {
                var isImageExist = IsImageExists(Url);
                if (isImageExist && !isCacheEnabled)
                {
                    DeleteFile(Url);
                }
                if (!isImageExist)
                {
                    await DownloadFile(Url);
                }
                var localUrl = GetLocalPath(Url);
                return localUrl;
            }
            catch
            {
                DeleteFile(Url);
                return null;
            }
        }

        private void DeleteFile(string url)
        {
            try
            {
                var localurl = GetLocalPath(url);
                File.Delete(localurl);
            }
            catch
            {

            }
        }

        private Task DownloadFile(string Url)
        {
            if (Downloads.ContainsKey(Url))
                return Downloads[Url];

            var task = Task.Run(async () =>
            {
                var fileName = Url.Split('/').Last();
                var appData = GetImagesFolderPath();
                var imagefilePath = Path.Combine(appData, fileName);

                using (var httpClient = new HttpClient())
                {
                    using (var fstream = File.Open(imagefilePath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var imstream = await httpClient.GetStreamAsync(Url))
                    {
                        await imstream.CopyToAsync(fstream);
                    }
                }
            });
            Downloads[Url] = task;
            return task;
        }

        private string GetImagesFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }
}
