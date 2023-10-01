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
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace ChatBotCalculatorV2
{
    public static class ChatCleanerBot
    {
        
        [FunctionName("ChatBotCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post","head", Route = null)] HttpRequest req,
            ILogger log)
        {
            string TelegramBotToken = await getSecret("TelegramBotToken");
            ITelegramBotClient bot = new TelegramBotClient(TelegramBotToken);
            Console.WriteLine("Activated bot" + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
            return new OkObjectResult("Ok");
        }

        public static async Task<string> getSecret(string keySecretName)
        {
            string keyVaultURL = "https://walletofkeys.vault.azure.net/";
            string clientID = "61a0661b-938b-4c1a-837a-39570660fefd";
            string tenantId = "b34aafba-3df0-425f-beef-f8de2163b096";
            string clientSecretId = "g5j8Q~B59YhTYMO9FCrSdSg360dNOwgLWxzsgc.H";
            
            var credential = new ClientSecretCredential(tenantId,clientID,clientSecretId);
            
            var client = new SecretClient(new Uri(keyVaultURL), credential);
            
            var secret = await client.GetSecretAsync(keySecretName);
            return (secret.Value.Value);
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

                var message = update.Message;
                string idTelegram = await getSecret("idTelegram");
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message &&
                    message.From.ToString() != idTelegram &&
                    update.Message.ReplyToMessage != null)
                {
                    if (update.Message.ReplyToMessage.MessageThreadId == 2)
                    {
                        await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                    }
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
