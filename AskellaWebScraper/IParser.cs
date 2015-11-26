using System.Collections.Generic;

namespace AskellaWebScraper
{
    public interface IParser
    {
        List<string> ParseHrefs(string html, string domainName);
    }
}
