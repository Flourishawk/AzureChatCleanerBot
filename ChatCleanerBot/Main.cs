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
            log.LogInformation("1 Log");
            string TelegramBotToken = await getSecret("WalletOfKeys","TelegramBotToken");
            log.LogInformation("2 Log");
            ITelegramBotClient bot = new TelegramBotClient(TelegramBotToken);
            log.LogInformation("3 Log");
            Console.WriteLine("Activated bot" + bot.GetMeAsync().Result.FirstName);
            log.LogInformation("4 Log");
            var cts = new CancellationTokenSource();
            log.LogInformation("5 Log");
            var cancellationToken = cts.Token;
            log.LogInformation("6 Log");
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            log.LogInformation("7 Log");
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            log.LogInformation("8 Log");
            Console.ReadLine();
            log.LogInformation("9 Log");
            return new OkObjectResult("Ok");
        }

        public static async Task<string> getSecret(string keyVaultName, string keySecretName)
        {
            string keyVaultUrl = $"https://{keyVaultName}.vault.azure.net";
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync(keySecretName);
            return (secret.Value.Value);
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            var message = update.Message;
            string idTelegram = await getSecret("WalletOfKeys","idTelegram");
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
