using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AskellaWebScraper
{
    public static class ConsoleWriter
    {
        private static BlockingCollection<string> _queue = new BlockingCollection<string>();

        static ConsoleWriter()
        {
            Task task = Task.Factory.StartNew
            (
                () => WriteOutput(_queue.Take())
             );
        }

        private static void WriteOutput(string message)
        {
            while (true)
            {
                Console.Out.WriteLineAsync(_queue.Take());
            }  
        }

        /// <summary>
        /// Puts a message on a queue, before writing to console.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            _queue.Add(message);
        }
    }
}
