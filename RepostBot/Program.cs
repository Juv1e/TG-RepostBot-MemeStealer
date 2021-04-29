using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RepostBot
{
    class Program
    {
        static TelegramBotClient Bot;
        static string token = "";
        static long channelId = 0;
        static int adminId = 0;
        static bool post_caption = false;
        static void Main(string[] args)
        {
            var strPath = AppDomain.CurrentDomain.BaseDirectory + "settings.json";
            if (!System.IO.File.Exists(strPath))
            {
                System.IO.File.Create(strPath).Dispose();
                System.IO.File.WriteAllText(strPath, "{\n    \"token\":\"\",\n    \"channel_id\":-0,\n    \"admin_id\":0,\n    \"post_caption\":false\n}");
                Console.WriteLine("Заполните данные в файле settings.json");
            }
            else
            {
                JObject parse = JObject.Parse(System.IO.File.ReadAllText(strPath));
                token = (string)parse["token"];
                channelId = (long)parse["channel_id"];
                adminId = (int)parse["admin_id"];
                post_caption = (bool)parse["post_caption"];
                Bot = new TelegramBotClient(token);
                var me = Bot.GetMeAsync().Result;
                Console.Title = me.Username;
                Bot.OnMessage += BotOnMessageReceived;
                Bot.StartReceiving(Array.Empty<UpdateType>());
                Console.WriteLine($"[TG] Успешная авторизация! @{me.Username}");
            }
            Console.ReadLine();
        }
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null) return;
            Console.WriteLine($"@{message.From.Username}[{message.From.LanguageCode}]: {message.Text} || {message.Chat.Id}");
            new Thread(() => Command(message)).Start();
        }
        public static void Command(Telegram.Bot.Types.Message message)
        {
            if (message.From.Id == adminId)
            {
                var photo = message.Photo ?? null;
                var video = message.Video ?? null;
                var gif = message.Animation ?? null;
                string caption;
                if (post_caption)
                    caption = message.Caption ?? "";
                else
                    caption = "";
                if (photo != null)
                {
                    Bot.SendPhotoAsync(channelId, photo.OrderByDescending(x => x.FileSize).First().FileId, caption);
                }
                else if (video != null)
                {
                    Bot.SendVideoAsync(channelId, video.FileId, caption: caption);
                }
                else if (gif != null)
                {
                    Bot.SendAnimationAsync(channelId, gif.FileId);
                }
                Bot.SendTextMessageAsync(message.From.Id, "Готово");
            }
            else
                Bot.SendTextMessageAsync(message.From.Id, "Сосеш....");
        }
    }
}
