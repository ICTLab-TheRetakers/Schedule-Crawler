using System;
using System.Threading.Tasks;
using WebCrawler.Components;

namespace WebCrawler
{
    class Program
    {
        static Crawler _crawler;

        static void Main(string[] args)
        {
            _crawler = new Crawler();
            Task.Run(async () =>
            {
                _crawler.StartCrawlingAsync();
            }).GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}
