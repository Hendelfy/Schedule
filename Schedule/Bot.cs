using Telegram.Bot;
using Telegram.Bot.Args;

namespace Schedule
{
    public static class Bot
    {
        static string ApiKey = ApiKeys.TelegramApiKey;
        public static readonly TelegramBotClient bot = new TelegramBotClient(ApiKey);

        public static void Init()
        {
            bot.OnMessage += BotOnOnMessage;
            bot.StartReceiving();
        }

        private static void BotOnOnMessage(object sender, MessageEventArgs e)
        {
            WhatICanDoWithThat(e.Message.Chat.Id, e.Message.Text);
        }

        private static void WhatICanDoWithThat(long chatId, string messageText)
        {
            if (chatId == ApiKeys.OwnersId)
            {
                if (messageText == "count")
                    Adm.Count();
                if(messageText =="all")
                    Adm.AllUsers();
            }

            if (messageText.Contains(" "))
                DataBaseActivity.CheckState(chatId, messageText);
            else
                switch (messageText.ToLower())
                {
                    case "/start":
                        DataBaseActivity.Registration(chatId);
                        break;
                    case "/change":
                        DataBaseActivity.Change(chatId);
                        break;
                    case "/refresh":
                        DataBaseActivity.RefreshBaseData(chatId);
                        break;
                    default:
                        DataBaseActivity.SomeDay(chatId, messageText);
                        break;
                }
        }
    }
}