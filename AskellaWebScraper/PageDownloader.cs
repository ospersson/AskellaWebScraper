using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace AskellaWebScraper
{
    public class PageDownloader : IPageDownloader
    {
        ConcurrentDictionary<string, byte[]> filesToPersistDict = new ConcurrentDictionary<string, byte[]>();

        public void DownloadPage(string url)
        {
            StartDownload(url);
        }

        private string _htmlFileName;
        public string HtmlFileName
        {
            set
            {
                _htmlFileName = value;
            }
        }

        private string _domainName;
        public string DomainName
        {
            set
            {
                _domainName = value;
            }
        }

        private string _downloadDirectory;
        public string DownloadDirectory
        {
            set
            {
                Directory.CreateDirectory(value);
                _downloadDirectory = value;
            }
        }

        private void StartDownload(string url)
        {
            string htmlFileName = @"" + _downloadDirectory + _htmlFileName;
            string html = string.Empty;

            //if head html exist don't download(used in development).
            if(!File.Exists(htmlFileName))
            {
                //Download head html file
                html = new WebClient().DownloadString(url);

                //Save file to disk
                File.WriteAllText(htmlFileName, html);
            }
            else
            {
                ConsoleWriter.WriteLine("Reading head html file from disk!");
                html = File.ReadAllText(htmlFileName);
            }

            IParser parser = new Parser();
            var listOfHrefs = parser.ParseHrefs(html, _domainName);

            var dictOfUrls = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(listOfHrefs, link =>
            {
                if (link == null) return;

                //We only need the key.
                var key = link.ToString();

                //Move to next item in dictionary if url exist.
                if (dictOfUrls.ContainsKey(key)) return;

                dictOfUrls.TryAdd(key, string.Empty);
            });

            ConsoleWriter.WriteLine("Parsing of href to list done with " + dictOfUrls.Count + " items");

            //Download all hrefs
            DownloadFromUrl(dictOfUrls.Keys.ToArray());
        }

        private void DownloadFromUrl(string[] listOfUrls)
        {
            var tasks = listOfUrls
                .Select(url => Task.Factory.StartNew(
                    state =>
                    {
                        using (var client = new WebClient())
                        {
                            byte[] content = null;
                            var urlToDownload = (string)state;

                            ConsoleWriter.WriteLine("Starting to download " + urlToDownload);

                            try
                            {
                                content = client.DownloadData(urlToDownload);
                            }
                            catch (WebException webEx)
                            {
                                //Todo: Study how exceptions behave in MT environment.
                                Console.WriteLine("-- WebException when downloding from Url : {0}", urlToDownload);
                                Console.WriteLine(webEx.InnerException);
                            }
                            catch (AuthenticationException aEx)
                            {
                                Console.WriteLine("-- AuthenticationException when downloding from Url : {0}", urlToDownload);
                                Console.WriteLine(aEx.InnerException);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("-- Exception when downloding from Url : {0}", urlToDownload);
                                Console.WriteLine(ex.InnerException);
                            }

                            ConsoleWriter.WriteLine("Finished downloading " + urlToDownload);

                            //Process file.
                            ProcessWrite(urlToDownload, content);

                            //Add files to dictionary. 
                            filesToPersistDict.TryAdd(urlToDownload, content);
                        }
                    }, url)
                )
                .ToArray();

            Task.WaitAll(tasks);
        }

        private void ProcessWrite(string url, byte[] content)
        {
            if (filesToPersistDict.ContainsKey(url)) return;

            try
            {
                ConsoleWriter.WriteLine("Processing : " + url);

                //Links or data can't be null
                if (String.IsNullOrEmpty(url)) return;
                if (content == null) return;
                    
                var uri = new Uri(url);

                string subfolderPath = _downloadDirectory;

                for (var i = 0; i < uri.Segments.Length; i++)
                {
                    var segments = uri.Segments.Length;

                    if (i == segments - 1) continue;

                    var folderName = uri.Segments[i].Replace("/", string.Empty);
                    if (folderName.Length != 0)
                    {
                        subfolderPath += (folderName + "\\");

                        CreateSubfolder(subfolderPath);
                    }

                    if (i == segments - 2)
                    {
                        //Save file to the subfolder or root
                        SaveFileToSubfolderOrRoot(content, uri, subfolderPath, i);
                    }   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("-- Exception when cleaning Url : {0}", url);
                Console.WriteLine(ex.InnerException);
            }
        }

        private static void SaveFileToSubfolderOrRoot(byte[] content, Uri uri, string subfolderPath, int i)
        {
            string fileName = uri.Segments[uri.Segments.Length - 1].Replace("/", string.Empty);
            string filePath = subfolderPath + fileName;
            try
            {
                FileToDisc.SaveToDisk(filePath, fileName, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("-- Exception in FileToDisk.SaveToDisk filepath : {0}", filePath + " " + fileName);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void CreateSubfolder(string subfolderPath)
        {
            try
            {
                Directory.CreateDirectory(subfolderPath);
                ConsoleWriter.WriteLine("Creating subfolder " + subfolderPath);
            }
            catch (Exception ex)
            {
                //A file with the same name as the subfolder could exist.
                if (File.Exists(subfolderPath))
                {
                    //File exist, rename it.
                    File.Move(subfolderPath, subfolderPath + ".html");

                    //Create the folder
                    Directory.CreateDirectory(subfolderPath);
                    ConsoleWriter.WriteLine("Creating subfolder(had to rename file)" + subfolderPath);
                }
                else
                {
                    Console.WriteLine("-- Exception in Directory.CreateDirectory path : {0}", subfolderPath);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
    }
}
