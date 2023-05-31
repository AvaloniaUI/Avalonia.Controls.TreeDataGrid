using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    internal class WikipediaPageViewModel
    {
        private readonly AvaloniaList<OnThisDayArticle> _data = new();

        public WikipediaPageViewModel()
        {
            var wrap = new TextColumnOptions<OnThisDayArticle>
            {
                TextTrimming = TextTrimming.None,
                TextWrapping = TextWrapping.Wrap,
            };

            Source = new FlatTreeDataGridSource<OnThisDayArticle>(_data)
            {
                Columns =
                {
                    new TemplateColumn<OnThisDayArticle>("Image", "WikipediaImageCell"),
                    new TextColumn<OnThisDayArticle, string?>("Title", x => x.Titles!.Normalized),
                    new TextColumn<OnThisDayArticle, string?>("Extract", x => x.Extract, GridLength.Star, wrap)
                }
            };

            _ = LoadContent();
        }

        public FlatTreeDataGridSource<OnThisDayArticle> Source { get; }

        private async Task LoadContent()
        {
            try
            {
                var client = new HttpClient();
                var d = DateTimeOffset.Now.Day;
                var m = DateTimeOffset.Now.Month;
                var uri = $"https://api.wikimedia.org/feed/v1/wikipedia/en/onthisday/all/{m}/{d}";
                var s = await client.GetStringAsync(uri);
                var data = JsonSerializer.Deserialize<OnThisDay>(s, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (data?.Selected is not null)
                    _data.AddRange(data.Selected.SelectMany(x => x.Pages!));
            }
            catch { }
        }
    }
}
