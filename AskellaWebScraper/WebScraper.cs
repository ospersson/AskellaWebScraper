using System;
using System.Diagnostics;
using System.Threading;

namespace AskellaWebScraper
{
    public class WebScraper
    {
        static void Main(string[] args)
        {
            //Fill in domain and url
            const string domainName = "mysite.com";
            const string url = "http://www.mysite.com";
            const string dir = @"C:\mysite\";

            string htmlFileName = domainName + ".html";

            IPageDownloader downloader = new PageDownloader()
            {
                DownloadDirectory = dir,
                HtmlFileName = htmlFileName,
                DomainName = domainName
            };

            Stopwatch sp = new Stopwatch();
            sp.Start();

            //Start the download
            downloader.DownloadPage(url);
            sp.Stop();

            Thread.Sleep(2000);

            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Askella Web Scraper finished with site: {0}", url);
            Console.WriteLine("Processing time in seconds: {0}", Math.Round(sp.Elapsed.TotalSeconds, 2).ToString());

            var delay = Console.ReadLine();
        }
    }
}
