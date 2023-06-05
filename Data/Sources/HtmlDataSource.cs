using Microsoft.Playwright;

/// <summary>
/// Note that in order to use this class you will first need to run the script that downloads headless  webkit, chromium, etc to your machine
/// You will get an exception with details on how to do this on first run
/// </summary>
public class HtmlDataSource : IDataSource
{
    public string Uri { get; private set; }

    public HtmlDataSource(string uri)
    {
        this.Uri = uri;
    }

    public async Task<IEnumerable<Resource>> Load()
    {
        var toReturn = new List<TextResource>();

        using var playwright = await Playwright.CreateAsync();
        {
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions{ UserAgent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.39"});
            await page.GotoAsync(this.Uri);

            var content = await page.TextContentAsync("body");

            toReturn.Add(new TextResource
            {
                Id = this.Uri,
                Value = content,
                ContentType = "text/html"
            });

        }        

        return toReturn;
    }
}