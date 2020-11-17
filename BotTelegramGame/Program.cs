using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace BotTelegramGame
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = File.ReadAllText(@"c:\token");
            //"1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"; 

            TelegramBotClient bot = new TelegramBotClient(token);
            Console.WriteLine($"@{bot.GetMeAsync().Result.Username} start...");
           
            int max = 5;
            Random rand = new Random();
            Dictionary<long, int> db = new Dictionary<long, int>();

            bot.OnMessage += (s, arg) =>
            {
                #region var 

                string msgText = arg.Message.Text;
                string firstName = arg.Message.Chat.FirstName;
                string replyMsg = String.Empty;
                int msgId = arg.Message.MessageId;
                long chatId = arg.Message.Chat.Id;

                int user = 0;
                string path = $"id_{chatId.ToString().Substring(0, 5)}";
                bool skip = false;

                Console.WriteLine($"{firstName}: {msgText}");

                #endregion

                #region Начало

                if (!db.ContainsKey(chatId) || msgText == "/restart")
                {
                    int startGame = rand.Next(20, 30);
                    db[chatId] = startGame;
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    skip = true;
                    replyMsg = $"Загадано число: {db[chatId]}";
                }

                #endregion

                else
                {
                    if (db[chatId] <= 0) return;

                    int.TryParse(msgText, out user);
                    if (!(user >= 1 && user <= max))
                    {
                        skip = true;
                        replyMsg = $"Обнаружено читерство. Число: {db[chatId]}";
                        replyMsg = "Обнаружено читерство. Число: " + db[chatId].ToString();
                        replyMsg = String.Format("Обнаружено читерство. Число: {0}",db[chatId]);
                    }
                    if (!skip)
                    {
                        db[chatId] -= user;

                        replyMsg = $"Ход {firstName}: {user}. Число: {db[chatId]}";
                        if (db[chatId] <= 0)
                        {
                            replyMsg = $"Ура! Победа, {firstName}!";
                            skip = true;
                        }
                    }
                }

                if (!skip)
                {
                    int temp = db[chatId] % (max + 1);
                    if (temp == 0) temp = rand.Next(max) + 1; // 1 2 3 4 5

                    db[chatId] -= temp;
                    replyMsg += $"\nХод БОТА: {temp} Число: {db[chatId]}";
                    if (db[chatId] <= 0) replyMsg = $"Ура! Победа БОТА!";
                }

                Bitmap image = new Bitmap(400, 400);
                Graphics graphics = Graphics.FromImage(image);

                graphics.DrawString(
                    s: replyMsg,
                    font: new Font("Consolas", 16),
                    brush: Brushes.Blue,
                    x: 10,
                    y: 200);

                path += $@"\file_{DateTime.Now.Ticks}.bmp";
                image.Save(path);

                

                bot.SendPhotoAsync(
                    chatId: chatId,
                    caption: replyMsg,

                    photo: new InputOnlineFile(new FileStream(path, FileMode.Open)),

                    replyToMessageId: msgId
                    );



                Console.WriteLine($"{replyMsg} \n");
                //bot.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: replyMsg,
                //    replyToMessageId: msgId
                //    );



            };

            bot.StartReceiving();
            Console.ReadLine();


        }
    }
}
