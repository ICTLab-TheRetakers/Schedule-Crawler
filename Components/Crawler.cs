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

        private async Task GetSchedule()
        {
            var url = "http://misc.hro.nl/roosterdienst/webroosters/CMI/kw3/15/r/r00024.htm";
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var table = document.DocumentNode.SelectNodes("/html/body/center/table[1]")[0];
            GetHours(3, table);
        }

        private void GetHours(int dayOfWeek, HtmlNode schedule)
        {
            var timeSchedule = new Schedule();
            timeSchedule.Days.Add(new Day());

            var rows = schedule.ChildNodes;
            for (int hours = 2; hours < rows.Count; hours = hours + 2)
            {
                var row = rows[hours];
                var currentHour = "";
                for (int i = 1; i < row.ChildNodes.Count; i = i + 2)
                {
                    if (i % 2 != 0)
                    {
                        var line = row.ChildNodes[i].InnerText;
                        if (!String.IsNullOrEmpty(line))
                        {
                            line = RemoveChars(line);
                            if (i == 1)
                            {
                                currentHour = line.Split(' ')[1];
                            }
                            if (line.Split(' ').Count() >= 2 && i > 1)
                            {
                                string classCode = "";
                                string teacherCode = "";
                                string courseCode = "";

                                if (line.Split(' ').Any(q => String.IsNullOrEmpty(q)))
                                {
                                    switch (line.Split(' ').Count())
                                    {
                                        case 2:
                                            classCode = "";
                                            teacherCode = "";
                                            courseCode = line.Split(' ')[0];

                                            break;
                                        case 3:
                                            if (line.Split(' ')[1].IndexOf("dag", StringComparison.OrdinalIgnoreCase) != -1)
                                            {
                                                classCode = "";
                                                teacherCode = "";
                                                courseCode = line.Split(' ')[0] + " " + line.Split(' ')[1];
                                            } else
                                            {
                                                classCode = line.Split(' ')[0];
                                                teacherCode = "";
                                                courseCode = line.Split(' ')[1];
                                            }

                                            break;
                                        case 4:
                                            if (String.IsNullOrEmpty(line.Split(' ')[2]) && String.IsNullOrEmpty(line.Split(' ')[3]))
                                            {
                                                classCode = line.Split(' ')[0];
                                                teacherCode = "";
                                                courseCode = line.Split(' ')[1];
                                            } else
                                            {
                                                classCode = line.Split(' ')[0];
                                                teacherCode = line.Split(' ')[1];
                                                courseCode = line.Split(' ')[2];
                                            }

                                            break;
                                        case 5:
                                            classCode = line.Split(' ')[0] + " " + line.Split(' ')[1];
                                            teacherCode = line.Split(' ')[2];
                                            courseCode = line.Split(' ')[3];

                                            break;
                                        default:
                                            break;
                                    }
                                }

                                var lessonCount = int.Parse(RemoveChars(row.ChildNodes[1].InnerText.Split(' ')[0]));
                                var lesson = new Hour(lessonCount);
                                lesson.Name = teacherCode + courseCode;
                                lesson.Class = classCode;
                                lesson.Course = courseCode;
                                lesson.Teacher = teacherCode;
                                lesson.StartTime = currentHour;

                                if (timeSchedule.Days.FirstOrDefault().Lessons.FirstOrDefault(q => q.Name == lesson.Name) == null)
                                {
                                    timeSchedule.Days.FirstOrDefault().Lessons.Add(lesson);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var lesson in timeSchedule.Days.First().Lessons)
            {
                if (lesson.Teacher == "" && lesson.Class == "")
                {
                    Console.WriteLine(String.Format("{0} begint vanaf {1}", lesson.Course, lesson.StartTime));
                } else if (lesson.Teacher == "")
                {
                    Console.WriteLine(String.Format("{0} tijdens {1} om {2}", lesson.Class, lesson.Course, lesson.StartTime));
                } else
                {
                    Console.WriteLine(String.Format("{0} gegeven door {1} aan klas {2} om {3}", lesson.Course, lesson.Teacher, lesson.Class, lesson.StartTime));
                }
            }

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

        

        private string[] GetDays(HtmlNode table)
        {
            List<string> daysOfWeek = new List<string>();
            var days = table.ChildNodes[1];

            for (int i = 0; i < days.ChildNodes.Count; i++)
            {
                var dayNode = days.ChildNodes[i];
                if (i >= 3 && i % 2 != 0)
                {
                    var day = dayNode.SelectNodes("table").FirstOrDefault()
                        .ChildNodes[0].ChildNodes[0].InnerText;

                    day = RemoveChars(day);
                    Console.WriteLine(day);

                    //Add day to list
                    daysOfWeek.Add(day);
                }
            }

            return daysOfWeek.ToArray();
        }

        private string RemoveChars(string text)
        {
            return text.Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty).Replace(".", String.Empty);
        }
    }
}
