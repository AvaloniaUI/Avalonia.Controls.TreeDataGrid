using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Models;
using Avalonia.Media.Imaging;

namespace TreeDataGridDemo.Models
{
    internal class OnThisDay
    {
        public OnThisDayEvent[]? Selected { get; set; }
    }

    internal class OnThisDayEvent
    {
        public string? Text { get; set; }
        public int Year { get; set; }
        public OnThisDayArticle[]? Pages { get; set; }
    }

    internal class OnThisDayArticle : NotifyingBase
    {
        private const string UserAgent = @"AvaloniaTreeDataGridSample/1.0 (https://avaloniaui.net; team@avaloniaui.net)";
        private bool _loadedImage;
        private Bitmap? _image;

        public string? Type { get; set; }
        public OnThisDayTitles? Titles { get; set; }
        public OnThisDayImage? Thumbnail { get; set; }
        public string? Description { get; set; }
        public string? Extract { get; set; }

        public Bitmap? Image
        {
            get
            {
                if (_image is null && !_loadedImage)
                {
                    _ = LoadImageAsync();
                }

                return _image;
            }
            private set => RaiseAndSetIfChanged(ref _image, value);
        }

        private async Task LoadImageAsync()
        {
            _loadedImage = true;

            if (Thumbnail?.Source is null)
                return;

            try
            {
                // Load the image from the url.
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                var bytes = await client.GetByteArrayAsync(Thumbnail!.Source);
                var s = new MemoryStream(bytes);
                Image = new Bitmap(s);
            }
            catch { }
        }
    }

    internal class OnThisDayTitles
    {
        public string? Normalized { get; set; }
    }

    internal class OnThisDayImage
    {
        public string? Source { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
