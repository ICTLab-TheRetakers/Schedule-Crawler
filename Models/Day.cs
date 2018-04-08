using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Models
{
    public class Day
    {
        public string DayofWeek { get; set; }
        public ICollection<Hour> Lessons { get; set; }

        public Day()
        {
            Lessons = new List<Hour>();
        }
    }
}
