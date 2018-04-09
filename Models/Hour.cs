using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models
{
    public class Hour
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

        public Hour(int id)
        {
            this.Id = id;
        }

        #endregion

    }
}
