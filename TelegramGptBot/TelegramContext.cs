using Telegram.Bot;
using Telegram.Bot.Types;
namespace TelegramGptBot.Telegram;

public class TelegramContext
{
    private readonly TelegramBotClient Bot;
    /// <summary>
    ///  Store Function that process a received message before responding to user
    /// </summary>
    public required Func<string,Task<Result<string, Exception>>> MessageProccessor;
    private CancellationTokenSource EndMonitoringTokens = new CancellationTokenSource();

    public TelegramBotConfig TelegramBotConfig { get; }

    public TelegramContext(TelegramBotConfig telegramBotConfig)
    {
        Bot = new TelegramBotClient(telegramBotConfig.TelegramBotToken);
        TelegramBotConfig = telegramBotConfig;
        Bot.DeleteWebhookAsync().Wait();
    }
    /// <summary>
    /// start a task that Monitor telegram message and respond to them 
    /// </summary>
    public void StartMessageMonitoring() {
        try
        {
            Task.Run(Monitoring);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - {ex.Message}");
        }
  
    }
    public async Task Monitoring() {
        PeriodicTimer messageChecker = new PeriodicTimer(TimeSpan.FromSeconds(TelegramBotConfig.ResponseIntervalInSecond));
        while (!EndMonitoringTokens.Token.IsCancellationRequested)
        {
            try
            {
                var messagesUpdates = await Bot.GetUpdatesAsync();
                await ProccessMessages(messagesUpdates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - {ex.Message}");
            }
            finally 
            {
                await messageChecker.WaitForNextTickAsync();
            }
        }
        messageChecker.Dispose();
    }
    public void StopMonitoring() // might come in use later 
    {        
        EndMonitoringTokens.Cancel();    
    }
   
    private async Task RespondeToMessage(long chatID,string Response)
    {
        await  Bot.SendTextMessageAsync(chatID, Response);        
    }
    private async Task ProccessMessages(Update[] messagesUpdates) 
    {

        foreach (var message in messagesUpdates)
        {
            if (EndMonitoringTokens.Token.IsCancellationRequested)
            {
                return;
            }
            if (IsMessageNotValid(message))
            {
                continue;
            }
            await ProccessAMessage(message.Message!);
        }
    }
    private async Task ProccessAMessage(Message message) 
    {
       
        try
        {
            var proccessedMessage = await MessageProccessor(message.Text!);
            if (!proccessedMessage.HasData())
            {
                await RespondeToMessage(message.Chat.Id!, "Failed to Process Message ");
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - GPT Request Exception {proccessedMessage.GetError()!.Message} from {message.Chat.Id}");
                return;
            }
            await RespondeToMessage(message.Chat.Id!, proccessedMessage.GetData()!);
        }
        catch (Exception ex)
        {            
            await RespondeToMessage(message.Chat.Id!, "Failed to Process Message ");
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - {ex.Message} from {message.Chat.Id}");
        } 

    }
    
    private bool IsMessageNotValid(Update message) {

        return message is null||message.Message is null || message.Message.Text is null ||
            (message.Message.Chat.Id != TelegramBotConfig.TelegramChatID && TelegramBotConfig.TelegramChatID is not null); // if there a specific chat id sat then prevent the bot from processing request from other chat
    }
}
