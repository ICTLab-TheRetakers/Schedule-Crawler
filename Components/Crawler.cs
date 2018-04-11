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
        public async void StartCrawlingAsync()
        {
            await GetSchedule();
        }

        #region Day and time logic

        //rows[2] = 08:30 - 09:20
        //rows[4] = 09:20 - 10:10
        //rows[6] = 10:30 - 11:20
        //rows[8] = 11:20 - 12:10
        //rows[10] = 12:10 - 13:00
        //rows[12] = 13:00 - 13:50
        //rows[14] = 13:50 - 14:40
        //rows[16] = 15:00 - 15:50
        //rows[18] = 15:50 - 16:40
        //rows[20] = 17:00 - 17:50
        //rows[22] = 17:50 - 18:40
        //rows[24] = 18:40 - 19:30
        //rows[26] = 19:30 - 20:20
        //rows[28] = 20:20 - 21:10
        //rows[30] = 21:10 - 22:00

        //ChildNodes[1] = tijd
        //ChildNodes[3] = les 1 op maandag
        //ChildNodes[5] = les 1 op dinsdag
        //ChildNodes[7] = les 1 op woensdag
        //ChildNodes[9] = les 1 op donderdag
        //ChildNodes[11] = les 1 op vrijdag

        #endregion

        #region Private Methods

        private async Task GetSchedule()
        {
            var url = "http://misc.hro.nl/roosterdienst/webroosters/CMI/kw3/15/r/r00028.htm";
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var table = document.DocumentNode.SelectNodes("/html/body/center/table[1]")[0];
            GetLessonByTime(table);
        }

        private void GetLessonByTime(HtmlNode schedule)
        {
            var timeSchedule = new Schedule();
            var addToNextRow = new List<LessonNextHour>();

            // Time rows from 08:30 to 22:00
            for (int time = 2; time < schedule.ChildNodes.Count; time += 2)
            {
                var row = schedule.ChildNodes[time];
                var lessons = row.ChildNodes.Where(n => n.Name == "td").ToList();
                var addPrevious = false;

                for (int lesson = 1; lesson < lessons.Count; lesson++)
                {
                    // Get current hour
                    var hourId = RemoveChars(lessons[0].InnerText.Split(' ')[0]);
                    var hour = RemoveChars(lessons[0].InnerText.Split(' ')[1]);

                    // Create new lesson and set start time
                    var newLesson = new Lesson(lesson);
                    newLesson.StartTime = hour;

                    // Get current lesson and info
                    var currentLesson = lessons[lesson];

                    // If contains key with tuple time and lesson, then add lesson of previous hour to this hour
                    if (lessons.Count != 6)
                    {
                        var id = Convert.ToInt32(String.Format("{0}{1}", time, lesson));
                        if (addToNextRow.FirstOrDefault(q => q.Hour == time
                        && q.Day == lesson && q.Id == id) != null)
                        {
                            var previousLesson = addToNextRow.FirstOrDefault(q => q.Hour == time && q.Day == lesson && q.Id == id);
                            if (previousLesson != null)
                            {
                                lessons.Insert(lesson, previousLesson.Lesson);
                                addToNextRow.Remove(previousLesson);

                                addPrevious = true;
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
                        if (addPrevious == true && timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons
                                .FirstOrDefault(q => q.StartTime == hour) == null)
                        {
                            timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons.Add(newLesson);
                            addPrevious = false;
                            continue;
                        }

                        // Add lesson to current day
                        if (timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons
                                .FirstOrDefault(q => q.StartTime == hour) == null)
                        {
                            timeSchedule.Days.FirstOrDefault(q => q.Id == lesson).Lessons.Add(newLesson);
                        }

                        // If row span is greater than two, than this lesson must be added at the next hour and same day
                        if (rowSpan > 2)
                        {
                            var difference = rowSpan - 2;
                            var totalLoops = difference / 2;
                            for (int j = 1; j < totalLoops + 1; j++)
                            {
                                var nextHour = (2 * j) + time;
                                if (addToNextRow.FirstOrDefault(q => q.Lesson == currentLesson && q.Day == lesson && q.Hour == nextHour) == null)
                                {
                                    var lessonNextHour = new LessonNextHour();
                                    lessonNextHour.Id = Convert.ToInt32(String.Format("{0}{1}", nextHour, lesson));
                                    lessonNextHour.Day = lesson;
                                    lessonNextHour.Hour = nextHour;
                                    lessonNextHour.Lesson = currentLesson;

                                    addToNextRow.Add(lessonNextHour);
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
