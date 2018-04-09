using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        #region Private Methods

        private async Task GetSchedule()
        {
            var url = "http://misc.hro.nl/roosterdienst/webroosters/CMI/kw3/15/r/r00028.htm";
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var table = document.DocumentNode.SelectNodes("/html/body/center/table[1]")[0];
            GetLessons(table);
        }

        private void GetLessons(HtmlNode schedule)
        {
            var timeSchedule = new Schedule();
            var rows = schedule.ChildNodes;
            var daysToSkip = new List<int>();

            for (int time = 2; time < rows.Count; time = time + 2)
            {
                var currentHour = "";
                var currentHourId = -1;
                var row = rows[time];

                for (int day = 3; day < row.ChildNodes.Count; day = day + 2)
                {
                    if (day % 2 != 0)
                    {
                        if (row.ChildNodes.Where(q => q.Name == "td").ToList().Count < 6)
                        {
                            if (daysToSkip.Contains(day))
                            {
                                daysToSkip.Remove(day);
                                continue;
                            }
                        }

                        var lessonNode = row.ChildNodes[day];
                        var rowSpan = int.Parse(lessonNode.Attributes.FirstOrDefault(q => q.Name == "rowspan").Value);

                        //Set current hour and hour id
                        currentHour = RemoveChars(row.ChildNodes[1].InnerText.Split(' ')[1]);
                        currentHourId = int.Parse(RemoveChars(row.ChildNodes[1].InnerText.Split(' ')[0]));

                        //Get lesson information
                        var lessonInfo = lessonNode.SelectSingleNode("table")
                            .ChildNodes
                            .Descendants("font")
                            .ToList();

                        //Create new lesson
                        var lesson = new Hour(currentHourId + day + time);
                        lesson.StartTime = currentHour;

                        if (lessonInfo != null)
                        {
                            //Set available information by looking at the count of elements in lessonInfo
                            switch (lessonInfo.Count)
                            {
                                case 0:
                                    lesson.Course = "Geen les";

                                    break;
                                case 1:
                                    lesson.Course = RemoveChars(lessonInfo[0].InnerText);

                                    break;
                                case 2:
                                    lesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                    lesson.Course = RemoveChars(lessonInfo[1].InnerText);

                                    break;
                                case 3:
                                    lesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                    lesson.Course = RemoveChars(lessonInfo[2].InnerText);
                                    lesson.Teacher = RemoveChars(lessonInfo[1].InnerText);

                                    break;
                            }
                        }

                        //If lesson not exists on current day, then add the lesson
                        if (timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons
                                .FirstOrDefault(q => q.StartTime == lesson.StartTime && q.Course == lesson.Course) == null)
                        {
                            timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons.Add(lesson);
                        }

                        if (rowSpan > 2)
                        {
                            var difference = rowSpan - 2;
                            var whiteSpace = 0;

                            switch (lessonInfo.Count)
                            {
                                case 1:
                                    whiteSpace = rowSpan - (2 * 1);
                                    break;
                                case 2:
                                    whiteSpace = rowSpan - (2 * 2);
                                    break;
                                case 3:
                                    whiteSpace = rowSpan - (2 * 3);
                                    break;
                                default:
                                    break;
                            }

                            var timesToLoop = difference / 2;
                            for (int i = 0; i < timesToLoop; i++)
                            {
                                var currentTimeRow = time;
                                while (currentTimeRow < rowSpan)
                                {
                                    //Increment by two
                                    currentTimeRow += 2;

                                    //Get time of lesson on current rowspan
                                    var nextHour = RemoveChars(rows[currentTimeRow].InnerText.Split(' ')[1]);

                                    if (whiteSpace > 0)
                                    {
                                        for (int j = 0; j < whiteSpace; j++)
                                        {
                                            if (j > 0)
                                            {
                                                currentTimeRow += 2;
                                                nextHour = RemoveChars(rows[currentTimeRow].InnerText.Split(' ')[1]);
                                            }

                                            //Create new lesson
                                            var nextLesson = new Hour(currentHourId + day + time);
                                            nextLesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                            nextLesson.Course = RemoveChars(lessonInfo[2].InnerText);
                                            nextLesson.Teacher = RemoveChars(lessonInfo[1].InnerText);
                                            nextLesson.StartTime = nextHour;

                                            //Add to day
                                            if (timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons
                                                .FirstOrDefault(q => q.StartTime == nextLesson.StartTime
                                                && q.Course == nextLesson.Course) == null)
                                            {
                                                timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons.Add(nextLesson);
                                            }
                                        }
                                    } else
                                    {
                                        //Create new lesson
                                        var nextLesson = new Hour(currentHourId + day + time);
                                        nextLesson.Class = RemoveChars(lessonInfo[0].InnerText);
                                        nextLesson.Course = RemoveChars(lessonInfo[2].InnerText);
                                        nextLesson.Teacher = RemoveChars(lessonInfo[1].InnerText);
                                        nextLesson.StartTime = nextHour;

                                        //Add to day
                                        if (timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons
                                            .FirstOrDefault(q => q.StartTime == nextLesson.StartTime
                                            && q.Course == nextLesson.Course) == null)
                                        {
                                            timeSchedule.Days.FirstOrDefault(q => q.Id == day).Lessons.Add(nextLesson);
                                        }
                                    }

                                    //Add to days to skip list
                                    if (!daysToSkip.Contains(day))
                                    {
                                        daysToSkip.Add(day);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Print all lessons
            PrintLessons(timeSchedule);

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
        }

        private void PrintLessons(Schedule schedule)
        {
            foreach (var day in schedule.Days)
            {
                Console.WriteLine("\n" + day.WeekDay + "\n---------------------------");
                foreach (var lesson in day.Lessons)
                {
                    if (String.IsNullOrEmpty(lesson.Teacher) && String.IsNullOrEmpty(lesson.Class) && String.IsNullOrEmpty(lesson.Teacher) && String.IsNullOrEmpty(lesson.Course))
                    {
                        //All properties are empty
                        Console.WriteLine("Geen les");
                    } else if (String.IsNullOrEmpty(lesson.Teacher) && String.IsNullOrEmpty(lesson.Class))
                    {
                        //Only course is not empty
                        Console.WriteLine(String.Format("{0} begint vanaf {1}", lesson.Course, lesson.StartTime));
                    } else if (String.IsNullOrEmpty(lesson.Teacher))
                    {
                        //Only teacher is empty
                        Console.WriteLine(String.Format("{0} tijdens {1} om {2}", lesson.Class, lesson.Course, lesson.StartTime));
                    } else
                    {
                        //All properties are not empty
                        Console.WriteLine(String.Format("{0} gegeven door {1} aan klas {2} om {3}", lesson.Course, lesson.Teacher, lesson.Class, lesson.StartTime));
                    }
                }
            }
        }

        private string RemoveChars(string text)
        {
            return text.Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty).Replace(".", String.Empty);
        }

        #endregion
    }
}
