using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Models
{
    public class Hour
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Teacher { get; set; }
        public string Class { get; set; }
        public string Course { get; set; }
        public string StartTime { get; set; }

        public Hour(int id)
        {
            this.Id = id;
        }
    }
}
