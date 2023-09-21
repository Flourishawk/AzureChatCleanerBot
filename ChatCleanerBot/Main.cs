using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace ChatBotCalculatorV2
{
    public static class ChatCleanerBot
    {
        
        [FunctionName("ChatBotCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string TelegramBotToken = getSecret("TelegramBotToken");
            ITelegramBotClient bot = new TelegramBotClient(TelegramBotToken);
            Console.WriteLine("Запущений бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
            return new OkResult();
        }

        public static string getSecret(string keySecretName)
        {
            string VaultUrl = $"https://walletofkeys.vault.azure.net/";
            var client = new SecretClient(new Uri(VaultUrl), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret(keySecretName);
            return (secret.Value);
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            var message = update.Message;
            string idTelegram = getSecret("idTelegram");
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message &&
                message.From.ToString() != idTelegram &&
                update.Message.ReplyToMessage != null)
            {
                if (update.Message.ReplyToMessage.MessageThreadId == 2)
                {
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                }
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
