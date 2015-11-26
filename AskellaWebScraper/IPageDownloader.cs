namespace AskellaWebScraper
{
    public interface IPageDownloader
    {
        void DownloadPage(string url);
        string DownloadDirectory { set; }
        string HtmlFileName { set; }
        string DomainName { set; }
    }
}