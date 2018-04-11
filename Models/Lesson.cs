using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models
{
    public class Lesson
    {
        #region Properties

        [Column(Order = 0), Key]
        public int Id { get; set; }
        public string Teacher { get; set; }
        public string Class { get; set; }
        public string Course { get; set; }
        [Column(Order = 1), Key]
        public string StartTime { get; set; }

        #endregion

        #region Constructors

        public Lesson(int id)
        {
            this.Id = id;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            string text = string.Empty;

            if (this.Course == "Geen les")
            {
                text = "Geen les";
            } else if (!String.IsNullOrEmpty(this.Course))
            {
                text = String.Format("{0} vanaf {1}", this.Course, this.StartTime);
            } else if (!String.IsNullOrEmpty(this.Course) && !String.IsNullOrEmpty(this.Class))
            {
                text = String.Format("{0} tijdens {1} om {2}", this.Class, this.Course, this.StartTime);
            } else if (!String.IsNullOrEmpty(this.Course) && !String.IsNullOrEmpty(this.Class))
            {
                text = String.Format("{0} tijdens {1} om {2}", this.Class, this.Course, this.StartTime);
            } else
            {
                text = String.Format("{0} gegeven door {1} aan klas {2} om {3}", this.Course, this.Teacher, this.Class, this.StartTime);
            }

            return text;
        }
        #endregion
    }
}
