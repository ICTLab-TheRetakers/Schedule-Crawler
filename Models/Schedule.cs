using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Models
{
    public class Schedule
    {
        public string Classroom { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Department { get; set; }
        public ICollection<Day> Days { get; set; }

        public Schedule()
        {
            this.Days = new List<Day>();
        }
    }
}
