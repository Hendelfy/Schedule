using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Schedule
{
    public static class Refreshing
    {
        static async Task<bool> ParseHtml(string response, User user) //monday = 1...
        {
            if (response == null)
            {
                await Bot.bot.SendTextMessageAsync(user.ChatId,
                    "Troubles with connection or yours personal information");
                return false;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            user.DailySubjectsList = new List<DailySubjects>();
            //var subCount = doc.DocumentNode.SelectNodes(@"/html/body/div/div[3]/div[2]/table[1]/tr").Count - 1;
            for (int j = 0, i = 0, rowNumber=0; j < 6; j++)
            {

                var dailySubjects = new DailySubjects() { Day = j, Subjects = new List<SubjModel>() };
                int dayRowSpan;
                int.TryParse(doc.DocumentNode.SelectSingleNode($"/html/body/div/div/div/div[3]/div[3]/table[1]/tr[{rowNumber + 2}]/th").Attributes["rowspan"].Value, out dayRowSpan);
                for (int k = 0; k < dayRowSpan; k++,i=0)
                {

                    var daysSchedule = doc.DocumentNode.SelectNodes($"/html/body/div/div/div/div[3]/div[3]/table[1]/tr[{rowNumber++ + 2}]/td");
                    foreach (var item in daysSchedule)
                    {
                        if (item.HasAttributes)
                        {
                            var subjModel = new SubjModel();
                            subjModel.Start = i;
                            subjModel.SubjectName = doc.DocumentNode.SelectSingleNode(item.XPath + "/b/abbr")
                                .Attributes["title"].Value;
                            var infoSubject = doc.DocumentNode.SelectSingleNode(item.XPath + "/small").InnerText
                                .Replace(" ", "").Split();
                            subjModel.Type = infoSubject[1].Substring(2);
                            subjModel.Cab = string.Concat(infoSubject[2].TakeWhile(x => x != '-'));
                            subjModel.Count = int.Parse(item.Attributes["colspan"].Value);
                            i += subjModel.Count;
                            dailySubjects.Subjects.Add(subjModel);
                        }
                        else i++;
                    }
                }

                dailySubjects.Subjects.Sort((x1,x2) => x1.Start.CompareTo(x2.Start));
                user.DailySubjectsList.Add(dailySubjects);
            }
            return true;
        }

        public static async Task<bool> Refresh(User user)
        {
            var client = new HttpClient() {BaseAddress = new Uri("https://is.vspj.cz/prihlasit")};
            if (await Auth(client, user.Login, user.Password))
                return await ParseHtml(await GetTable(client), user);
            return false;
        }

        static async Task<bool> Auth(HttpClient client, string username, string password)
        {
            try
            {
                var certificate = await GetCertificate(client);
                var parameters = new Dictionary<string, string>()
                {
                    {"_username", username},
                    {"_password", password},
                    {"_csrf_token", certificate}
                };
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var content = new FormUrlEncodedContent(parameters);
                await client.PostAsync(client.BaseAddress, content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        static async Task<string> GetCertificate(HttpClient client)
        {
            var response1 = await client.GetAsync(client.BaseAddress);
            var certificate = await response1.Content.ReadAsStringAsync();
            certificate = certificate.Substring(certificate.IndexOf("csrf_token\" value=\"") + 19, 43);
            return certificate;
        }

        static async Task<string> GetTable(HttpClient client)
        {
            string response;
            try
            {
                var pageWithSchedule = await client.GetAsync(new Uri("https://isz.vspj.cz/student/rozvrh/muj-rozvrh"));
                response = await pageWithSchedule.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            if (!response.Contains("Rozvrh"))
            {
                return null;
            }

            return response;
        }
    }
}