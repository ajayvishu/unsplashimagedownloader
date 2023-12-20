using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace UnsplashImageDownloader
{
    public partial class Unsplash : Form
    {
        public Unsplash()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = true;
            label1.Text = "Downloading Images....";

            List<string> imageUrls = await GenerateHighResolutionImageUrlsAsync(5);

            string downloadFolder = @"D:\AJAY\Unsplash";

            await DownloadImagesAsync(imageUrls, downloadFolder);

            Application.Exit();
        }

        static async Task<List<string>> GenerateHighResolutionImageUrlsAsync(int count)
        {
            List<string> imageUrls = new List<string>();
            string apiKey = "SOKtnhNsde_f-f2Hf0Oa2K3elwscYxP-VyDSa_dZ1cE"; // Replace with your Unsplash API key

            using (HttpClient client = new HttpClient())
            {
                for (int i = 0; i < count; i++)
                {
                    string apiUrl = $"https://api.unsplash.com/photos/random?query=developer&orientation=landscape&client_id={apiKey}&w=1600&h=900";

                    try
                    {
                        var response = await client.GetStringAsync(apiUrl);

                        // Deserialize the JSON response to get the image URL
                        var unsplashImage = JsonConvert.DeserializeObject<UnsplashImage>(response);
                        if (unsplashImage != null && unsplashImage.Urls != null)
                        {
                            imageUrls.Add(unsplashImage.Urls.Regular);
                        }
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("403"))
                    {
                        // Handle 403 Forbidden (Rate Limit Exceeded) by introducing a delay
                        Console.WriteLine("Rate limit exceeded. Waiting for 60 minutes.");
                        await Task.Delay(600000); // Wait for 60 minutes before retrying
                        i--; // Retry the same request after the delay
                    }
                }
            }

            return imageUrls;
        }

        static async Task DownloadImagesAsync(List<string> imageUrls, string downloadFolder)
        {
            using (HttpClient client = new HttpClient())
            {
                var tasks = new List<Task>();

                foreach (var imageUrl in imageUrls)
                {
                    tasks.Add(DownloadImageAsync(client, imageUrl, downloadFolder));
                }

                await Task.WhenAll(tasks);
            }
        }

        static async Task DownloadImageAsync(HttpClient client, string imageUrl, string downloadFolder)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    string fileExtension = GetFileExtensionFromUrl(imageUrl);
                    string fileName = Guid.NewGuid().ToString() + fileExtension;

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                  stream = new FileStream(Path.Combine(downloadFolder, fileName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image from {imageUrl}: {ex.Message}");
            }
        }

        static string GetFileExtensionFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string path = uri.LocalPath;
            int lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex != -1)
            {
                return path.Substring(lastDotIndex);
            }
            return ".jpg"; // Default to .jpg if no extension found
        }

        static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
    }

    public class UnsplashImage
    {
        public UnsplashUrls Urls { get; set; }
    }

    public class UnsplashUrls
    {
        public string Regular { get; set; }
    }
}
