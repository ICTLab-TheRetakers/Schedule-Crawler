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
                try
                {
                    await _crawler.StartCrawlingAsync("r00028");
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }).GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}
