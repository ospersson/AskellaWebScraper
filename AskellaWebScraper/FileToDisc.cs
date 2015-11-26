using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace AskellaWebScraper
{
    public static class FileToDisc
    {
        private static BlockingCollection<FileDto> _queue = new BlockingCollection<FileDto>();

        static FileToDisc()
        {
            Task task = Task.Factory.StartNew
            (
               () => WriteToDisc()
            );
        }
        
        private static void WriteToDisc()
        {
            while (true)
            {
                var file = _queue.Take();

                try
                {
                    using (var sourceStream = new FileStream(
                        file.Path, FileMode.Append, FileAccess.Write, FileShare.Write,
                        bufferSize: 4096, useAsync: true))
                    {
                        Task theTask = sourceStream.WriteAsync(file.Content, 0, file.Content.Length);
                        ConsoleWriter.WriteLine("Saving to disk: " + file.Path);
                    }
                }
                catch (IOException iex)
                {
                    ConsoleWriter.WriteLine(iex.Message);
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds the file to the save queue.
        /// </summary>
        /// <param name="path">Save to this path</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="content">Byte array of data to save</param>
        public static void SaveToDisk(string path, string fileName, byte[] content)
        {
            _queue.Add(new FileDto { Path = path, FileName = fileName, Content = content });
        }

        private class FileDto
        {
            public string Path { get; set; }
            public string FileName { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
