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
            _crawler = new Crawler("CMI", 15, 3);
            Task.Run(async () =>
            {
                await _crawler.StartCrawlingAsync("r00024");

            }).GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}
