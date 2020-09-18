using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Telegram.Bot.Types.ReplyMarkups;

namespace Schedule
{
    public static class DataBaseActivity
    {
        public static async void RefreshBaseData(long chatId)
        {
            using (var db = new LiteDatabase("db.db"))
            {
                var collection = db.GetCollection<User>("users");
                var user = collection.FindOne(x => x.ChatId == chatId);
                var flag = await Refreshing.Refresh(user);
                if (flag)
                    collection.Update(user);
                else return;
            }

            var keyboard = new ReplyKeyboardMarkup()
            {
                Keyboard = new[]
                {
                    new[]
                    {
                        new KeyboardButton("Monday"),
                        new KeyboardButton("Tuesday"),
                        new KeyboardButton("Wednesday")
                    },
                    new[]
                    {
                        new KeyboardButton("Thursday"),
                        new KeyboardButton("Friday"),
                        new KeyboardButton("Saturday")
                    }
                },
                ResizeKeyboard = true
            };
            await Bot.bot.SendTextMessageAsync(chatId, "Days", replyMarkup: keyboard);
        }

        public static async void Registration(long chatId)
        {
            using (var db = new LiteDatabase("db.db"))
            {
                var collection = db.GetCollection<User>("users");
                var user = collection.FindOne(x => x.ChatId == chatId);
                if (user == null)
                {
                    user = new User() {ChatId = chatId, State = 0};
                    collection.Insert(user);
                    await Bot.bot.SendTextMessageAsync(chatId,
                        "U've been registered!" + Environment.NewLine +
                        "Send your login and password in one message with whitespace");
                }
                else await Bot.bot.SendTextMessageAsync(chatId, "U are registered already");
            }
        }

        public static async void CheckState(long chatId, string message)
        {
            using (var db = new LiteDatabase("db.db"))
            {
                var collection = db.GetCollection<User>("users");
                var user = collection.FindOne(x => x.ChatId == chatId);
                if (user.State == 0)
                {
                    var splittedMessage = message.Split();
                    if (splittedMessage.Length == 2)
                    {
                        user.Login = splittedMessage[0];
                        user.Password = splittedMessage[1];
                        user.State = 1;
                        collection.Update(user);
                        await Bot.bot.SendTextMessageAsync(chatId, "Use: \"/refresh\"");
                    }
                }
                else await Bot.bot.SendTextMessageAsync(chatId, "If u wanna change your data, Use: \"/change\"");
            }
        }

        public static async void Change(long chatId)
        {
            using (var db = new LiteDatabase("db.db"))
            {
                var collection = db.GetCollection<User>("users");
                var user = collection.FindOne(x => x.ChatId == chatId);
                user.State = 0;
                collection.Update(user);
                await Bot.bot.SendTextMessageAsync(chatId,
                    "Send yours login and password in one message with whitespace");
            }
        }

        public static async void SomeDay(long chatId, string message)
        {
            int dayNumber;
            switch (message.ToLower())
            {
                case "monday":
                    dayNumber = 0;
                    break;
                case "tuesday":
                    dayNumber = 1;
                    break;
                case "wednesday":
                    dayNumber = 2;
                    break;
                case "thursday":
                    dayNumber = 3;
                    break;
                case "friday":
                    dayNumber = 4;
                    break;
                case "saturday":
                    dayNumber = 5;
                    break;
                default:
                    await Bot.bot.SendTextMessageAsync(chatId, "Wrong Day");
                    return;
            }

            using (var db = new LiteDatabase("db.db"))
            {
                string result = string.Empty;
                var collection = db.GetCollection<User>("users");
                var user = collection?.FindOne(x => x.ChatId == chatId);
                if (user == null)
                {
                    await Bot.bot.SendTextMessageAsync(chatId, "U are not registered. Type /start");
                    return;
                }

                if (user.DailySubjectsList == null)
                {
                    await Bot.bot.SendTextMessageAsync(chatId, "Use /refresh");
                    return;
                }

                foreach (var item in user.DailySubjectsList[dayNumber].Subjects)
                {
                    result += item.SubjectName + " | " + item.Type + Environment.NewLine + item.Cab +
                              $" ({GetFloor(item.Cab)})" +
                              Environment.NewLine +
                              GetTiming(item.Start, item.Count) + $" | ({item.Start}-{item.Count + item.Start - 1})" +
                              Environment.NewLine + Environment.NewLine;
                }

                if (result == string.Empty)
                    result = "Have nothing";
                await Bot.bot.SendTextMessageAsync(chatId, result);
            }
        }

        static string GetTiming(int start, int count)
        {
            Tuple<TimeSpan, TimeSpan>[] timing = new[]
            {
                Tuple.Create(new TimeSpan(7, 10, 0), new TimeSpan(7, 55, 0)),
                Tuple.Create(new TimeSpan(8, 0, 0), new TimeSpan(8, 45, 0)),
                Tuple.Create(new TimeSpan(8, 50, 0), new TimeSpan(9, 35, 0)),
                Tuple.Create(new TimeSpan(9, 45, 0), new TimeSpan(10, 30, 0)),
                Tuple.Create(new TimeSpan(10, 35, 0), new TimeSpan(11, 20, 0)),
                Tuple.Create(new TimeSpan(11, 30, 0), new TimeSpan(12, 15, 0)),
                Tuple.Create(new TimeSpan(12, 35, 0), new TimeSpan(13, 20, 0)),
                Tuple.Create(new TimeSpan(13, 30, 0), new TimeSpan(14, 15, 0)),
                Tuple.Create(new TimeSpan(14, 20, 0), new TimeSpan(15, 05, 0)),
                Tuple.Create(new TimeSpan(15, 15, 0), new TimeSpan(16, 0, 0)),
                Tuple.Create(new TimeSpan(16, 05, 0), new TimeSpan(16, 50, 0)),
                Tuple.Create(new TimeSpan(17, 0, 0), new TimeSpan(17, 45, 0)),
                Tuple.Create(new TimeSpan(17, 50, 0), new TimeSpan(18, 35, 0)),
                Tuple.Create(new TimeSpan(18, 45, 0), new TimeSpan(19, 30, 0)),
            };
            return $"{timing[start].Item1:hh\\:mm} - {timing[start + count - 1].Item2:hh\\:mm}";
        }

        static string GetFloor(string cab)
        {
            string floor;
            switch (cab)
            {
                case "U18":
                case "V5":
                case "U5":
                case "AUL":
                case "ZAS":
                case "V4":
                case "V3":
                case "U3V":
                case "P3":
                case "U1":
                case "U4":
                case "ZSP":
                case "UY":
                case "Lab. EM":
                    floor = "1st floor";
                    break;
                case "U12":
                case "U11":
                case "U10":
                case "V8":
                case "P1":
                case "J3":
                case "J2":
                case "P4":
                case "U9":
                case "U8":
                case "U7":
                case "U6":
                    floor = "2nd floor";
                    break;
                case "ZDR1":
                case "J1":
                case "V2":
                case "V1":
                case "ZSR2":
                case "EIT":
                case "Lab.":
                case "AMT":
                case "ROB":
                case "U17":
                case "U16":
                case "U15":
                case "U14":
                    floor = "3rd floor";
                    break;
                case "Lab. ZSM":
                case "V9":
                    floor = "0 floor";
                    break;
                default:
                    floor = "";
                    break;
            }

            return floor;
        }
    }
}