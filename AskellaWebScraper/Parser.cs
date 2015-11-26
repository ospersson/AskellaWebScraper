using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AskellaWebScraper
{
    internal class Parser : IParser
    {
        /// <summary>
        /// Uses regexp to parse out Hrefs from a string of html.
        /// Inspired by: https://msdn.microsoft.com/en-us/library/t9e807fx(v=vs.110).aspx
        /// </summary>
        /// <param name="html"></param>
        /// <param name="html"></param>
        /// <returns>List of parsed Urls</returns>
        public List<string> ParseHrefs(string html, string domainName)
        {
            var listOfHrefs = new List<string>();

            string HRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

            try
            {
                var m = Regex.Match(html, HRefPattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));

                while (m.Success)
                {
                    var href = m.Groups[1].ToString();

                    //Check if link is for this domain.
                    if (!href.Contains(domainName) || href.Contains("xmlrpc.php"))
                    {
                        ConsoleWriter.WriteLine("Skipping " + href);
                    }
                    else
                    {
                        listOfHrefs.Add(href);
                    }

                    m = m.NextMatch();
                }
            }
            catch (RegexMatchTimeoutException)
            {
                ConsoleWriter.WriteLine("The matching operation timed out.");
            }

            return listOfHrefs;
        }
    }
}
