using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;

namespace WebCrawler.Models
{
    public class LessonNextHour
    {
        [Key]
        public int Id { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }
        public HtmlNode Lesson { get; set; }
    }
}
