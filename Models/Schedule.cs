using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models
{
    public class Schedule
    {
        #region Properties

        [Key]
        public string Classroom { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Department { get; set; }
        public ICollection<Day> Days { get; set; }

        #endregion

        #region Constructors

        public Schedule()
        {
            this.Days = new List<Day>()
            {
                new Day(1),
                new Day(2),
                new Day(3),
                new Day(4),
                new Day(5)
            };
        }

        #endregion
    }
}
