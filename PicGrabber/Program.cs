using System.Text.RegularExpressions;

namespace PicGrabber
{
    internal abstract partial class Program
    {
        private const string BaseOutputFolder = @"C:\Users\David\Downloads\grab";

        private static readonly string[] Urls =
        [
            "https://heroesofthestorm.fandom.com/wiki/Illidan",
            "https://heroesofthestorm.fandom.com/wiki/Jaina",
            "https://heroesofthestorm.fandom.com/wiki/Murky",
            "https://heroesofthestorm.fandom.com/wiki/Sylvanas"
        ];

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting image download and processing...");
            foreach (var url in Urls)
            {
                Console.WriteLine($"Processing URL: {url}");
                try
                {
                    await DownloadAndProcessImages(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {url}: {ex.Message}");
                }
            }
            Console.WriteLine("Processing complete.");
        }

        private static async Task DownloadAndProcessImages(string url)
        {
            var heroName = url.Split('/').Last();
            var emojiFolder = Path.Combine(BaseOutputFolder, heroName, "Emojis");
            var sprayFolder = Path.Combine(BaseOutputFolder, heroName, "Sprays");

            Directory.CreateDirectory(emojiFolder);
            Directory.CreateDirectory(sprayFolder);

            using var client = new HttpClient();

            Console.WriteLine($"Downloading HTML content for {heroName}...");
            var html = await client.GetStringAsync(url);

            var imageUrls = ExtractImageUrls(html);

            foreach (var imageUrl in imageUrls)
            {
                var fileName = Path.GetFileName(imageUrl.Split('?').First());
                if (fileName.Contains("spray", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Downloading spray: {imageUrl}");
                    await DownloadImage(imageUrl, sprayFolder);
                }
                else if (fileName.Contains("emoji", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Downloading emoji: {imageUrl}");
                    await DownloadImage(imageUrl, emojiFolder);
                }
            }
        }

        private static HashSet<string> ExtractImageUrls(string html)
        {
            var imageUrls = new HashSet<string>();
            var matches = MyRegex().Matches(html);

            foreach (Match match in matches)
            {
                if (Uri.TryCreate(match.Value, UriKind.Absolute, out var uri))
                {
                    imageUrls.Add(uri.ToString());
                }
            }

            return imageUrls;
        }

        private static async Task DownloadImage(string imageUrl, string folder)
        {
            using var client = new HttpClient();
            var fileName = Path.GetFileName(imageUrl.Split('?').First());
            var filePath = Path.Combine(folder, fileName);

            try
            {
                var imageBytes = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Console.WriteLine($"Image downloaded: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download {imageUrl}: {ex.Message}");
            }
        }

        [GeneratedRegex(@"https?://[^\s""]+\.(png|gif)")]
        private static partial Regex MyRegex();
    }
}
