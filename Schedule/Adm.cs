using System;
using System.Linq;
using LiteDB;

namespace Schedule
{
    static class Adm
    {
        private static long ChatId = ApiKeys.OwnersId;

        public static async void Count()
        {
            int count = -1;
            using (var db = new LiteDatabase("db.db"))
            {
                count = db.GetCollection<User>("users").FindAll().Count();
            }

            await Bot.bot.SendTextMessageAsync(ChatId, count.ToString());
        }
        public static async void AllUsers()
        {
            string res = string.Empty;
            using (var db = new LiteDatabase("db.db"))
            {
                var users = db.GetCollection<User>("users").FindAll();
                foreach (var item in users)
                {
                    res += item.Login+Environment.NewLine;
                }
            }

            await Bot.bot.SendTextMessageAsync(ChatId, res);
        }
    }
}