using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebCrawler.Models;

namespace WebCrawler.Components
{
    public class Crawler
    {
        #region Properties

        private string Department;
        private int Week;
        private int Quarter;

        #endregion

        #region Constructor

        public Crawler(string department, int week, int quarter)
        {
            this.Department = department;
            this.Week = week;
            this.Quarter = quarter;
        }

        #endregion

        #region Public Methods

        public async Task StartCrawlingAsync(string room)
        {
            await GetSchedule(this.Quarter, this.Week, this.Department, room);
        }

        #endregion

        #region Private Methods

        private async Task GetSchedule(int quarterOfYear, int week, string department, string room)
        {
            var url = String.Format("http://misc.hro.nl/roosterdienst/webroosters/{0}/kw{1}/{2}/r/{3}.htm", department, quarterOfYear, week, room);
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var table = document.DocumentNode.SelectNodes("/html/body/center/table[1]")[0];
            GetLessons(table);
        }

        private void GetLessons(HtmlNode schedule)
        {
            // Create list for multi hour lessons
            var lessonsToAddNextHour = new List<LessonOnNextRow>();

            //Create new schedule and set properties
            var timeSchedule = new Schedule();
            timeSchedule.Department = this.Department;
            timeSchedule.Week = this.Week;
            timeSchedule.QuarterOfYear = this.Quarter;

            // Loop through each row (row is an hour, e.x. 08:30-09:20)
            for (int time = 2; time < schedule.ChildNodes.Count; time += 2)
            {
                var addMultiHourLesson = false;

                // Get hour
                //Get lessons from current hour
                var row = schedule.ChildNodes[time];
                var lessons = row.ChildNodes.Where(n => n.Name == "td").ToList();

                for (int lesson = 1; lesson < lessons.Count; lesson++)
                {
                    // Get current hour
                    var hourNumber = RemoveChars(lessons[0].InnerText.Split(' ')[0]);
                    var currentHour = RemoveChars(lessons[0].InnerText.Split(' ')[1]);

                    // Create new lesson and set start time
                    var newLesson = new Lesson(lesson);
                    newLesson.StartTime = currentHour;

                    // Get current lesson and info
                    var currentLesson = lessons[lesson];

                    // If contains key with tuple time and lesson, then add lesson of previous hour to this hour
                    if (lessons.Count != 6)
                    {
                        var id = Convert.ToInt32(String.Format("{0}{1}", time, lesson));
                        if (lessonsToAddNextHour.FirstOrDefault(q => q.Hour == time && q.Day == lesson && q.Id == id) != null)
                        {
                            var previousLesson = lessonsToAddNextHour.FirstOrDefault(q => q.Hour == time && q.Day == lesson && q.Id == id);
                            if (previousLesson != null)
                            {
                                lessons.Insert(lesson, previousLesson.Lesson);

                                // Remove lesson from list
                                lessonsToAddNextHour.Remove(previousLesson);

                                // Set multi hour lesson to true
                                addMultiHourLesson = true;
                                currentLesson = previousLesson.Lesson;
                            }
                        }
                    }

                    // Get lesson and info
                    var lessonInfo = currentLesson.SelectSingleNode("table").ChildNodes.Descendants("font").ToList();

                    // Get lesson row span
                    var rowSpan = Convert.ToInt32(currentLesson.Attributes["rowspan"].Value);

                    if (lessonInfo != null)
                    {
                        // Set lesson properties
                        switch (lessonInfo.Count)
                        {
                            case 0:
                                newLesson.Course = "Geen les";
                                newLesson.Class = String.Empty;
                                newLesson.Teacher = String.Empty;
                                break;
                            case 1:
                                newLesson.Course = RemoveChars(lessonInfo[0].InnerText);
                                newLesson.Class = String.Empty;
                                newLesson.Teacher = String.Empty;
                                break;
                            case 2:
                                newLesson.Course = RemoveChars(lessonInfo[1].InnerText);
                                newLesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                newLesson.Teacher = String.Empty;
                                break;
                            case 3:
                                newLesson.Course = RemoveChars(lessonInfo[2].InnerText);
                                newLesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                newLesson.Teacher = RemoveChars(lessonInfo[1].InnerText);
                                break;
                        }

                        // Add lesson to current day
                        if (addMultiHourLesson == true && timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons
                                .FirstOrDefault(q => q.StartTime == currentHour) == null)
                        {
                            timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons.Add(newLesson);

                            // Reset multi hour lesson
                            addMultiHourLesson = false;

                            // Continue to next day
                            continue;

                        } else if (timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons
                                .FirstOrDefault(q => q.StartTime == currentHour) == null)
                        {
                            timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons.Add(newLesson);
                        }

                        // If row span is greater than two, 
                        // than the current lesson must be added at the next hour and same day
                        if (rowSpan > 2)
                        {
                            var difference = rowSpan - 2;
                            var totalLoops = difference / 2;

                            for (int j = 1; j < totalLoops + 1; j++)
                            {
                                var nextHour = (2 * j) + time;

                                if (lessonsToAddNextHour.FirstOrDefault(q => q.Lesson == currentLesson && q.Day == lesson && q.Hour == nextHour) == null)
                                {
                                    var lessonNextHour = new LessonOnNextRow();
                                    lessonNextHour.Id = Convert.ToInt32(String.Format("{0}{1}", nextHour, lesson));
                                    lessonNextHour.Day = lesson;
                                    lessonNextHour.Hour = nextHour;
                                    lessonNextHour.Lesson = currentLesson;

                                    lessonsToAddNextHour.Add(lessonNextHour);
                                }
                            }
                        }
                    }
                }
            }

            Print(timeSchedule);
        }

        private string GetDay(int day)
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
            }

            return dayOfWeek;
        }

        private void Print(Schedule schedule)
        {
            foreach (var day in schedule.Days)
            {
                Console.WriteLine(day.WeekDay + "\n---------------------------");
                foreach (var lesson in day.Lessons)
                {
                    Console.WriteLine(lesson.ToString());
                }
                Console.WriteLine("\n\n");
            }
        }

        private string RemoveChars(string text)
        {
            return text.Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty).Replace(".", String.Empty);
        }

        #endregion
    }
}
