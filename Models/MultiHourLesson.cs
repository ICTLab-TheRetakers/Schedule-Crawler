using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;

namespace WebCrawler.Models
{
    public class MultiHourLesson
    {
        #region Properties

        [Key]
        public string Id { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }
        public HtmlNode Lesson { get; set; }

        #endregion

        #region Constructors

        public MultiHourLesson() {}

        public MultiHourLesson(string id, int hour, int day, HtmlNode lesson)
        {
            this.Id = id;
            this.Hour = hour;
            this.Day = day;
            this.Lesson = lesson;
        }

        #endregion
    }
}
