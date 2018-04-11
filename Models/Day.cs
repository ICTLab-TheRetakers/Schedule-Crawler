using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models
{
    public class Day
    {
        #region Properties

        [Key]
        public int Id { get; set; }
        public string WeekDay { get; set; }
        public List<Lesson> Lessons { get; set; }

        #endregion

        #region Constructors

        public Day()
        {
            this.Lessons = new List<Lesson>();
        }

        public Day(int id)
        {
            this.Id = id;
            Lessons = new List<Lesson>();
            this.SetDay(this.Id);
        }

        #endregion

        #region Setters

        public void SetDay(int day)
        {
            this.WeekDay = this.GetDay(day);
        }

        #endregion

        #region Getters

        public string GetDay(int day)
        {
            string dayOfWeek = "";
            switch (day)
            {
                case 1:
                    dayOfWeek = "Monday";
                    break;
                case 2:
                    dayOfWeek = "Tuesday";
                    break;
                case 3:
                    dayOfWeek = "Wednesday";
                    break;
                case 4:
                    dayOfWeek = "Thursday";
                    break;
                case 5:
                    dayOfWeek = "Friday";
                    break;
                default:
                    dayOfWeek = "Monday";
                    break;
            }

            return dayOfWeek;
        }

        #endregion
    }
}
