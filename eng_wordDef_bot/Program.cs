using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Configuration;

namespace eng_wordDef_bot
{
    class Program
    {
        private static string bot_key;
        private static string app_id;
        private static string app_key;
        private static ITelegramBotClient botClient;
        private static Uri _pathToApiSerivce = new Uri("https://od-api.oxforddictionaries.com:443/api/v1/entries/");
        private static HttpClient httpClient;
        static void Main(string[] args)
        {
            bot_key = ConfigurationManager.AppSettings["bot_key"];
            app_id = ConfigurationManager.AppSettings["app_id"];
            app_key = ConfigurationManager.AppSettings["app_key"];

            botClient = new TelegramBotClient(bot_key);
            httpClient = new HttpClient();
            httpClient.BaseAddress = _pathToApiSerivce;

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("app_id", app_id);
            httpClient.DefaultRequestHeaders.Add("app_key", app_key);
            
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(Timeout.Infinite);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {

            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}. Message \"{e.Message.Text}\"");

                var defenition = FindDefenition(e.Message.Text);

                await botClient.SendTextMessageAsync(
                  chatId: e.Message.Chat,
                  text: "Defenition is:\n" + defenition
                );
            }
        }

        static string FindDefenition(string word)
        {
            
            if (String.IsNullOrWhiteSpace(word)||word.IndexOf(' ')!=-1)
                return "Invalid request, please try type a word";
            try
            {
                var jsonData = httpClient.GetStringAsync($"en/{word}").Result;

                RootObject data = JsonConvert.DeserializeObject<RootObject>(jsonData);

                return data.results[0].lexicalEntries[0].entries[0].senses[0].definitions[0];
            }
            catch (Exception ex)
            {
                return "Invalid request, please try a new one";
            }   
        }
    }
}
