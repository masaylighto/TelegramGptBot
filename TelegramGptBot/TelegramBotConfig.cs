
namespace TelegramGptBot.Telegram;

public class TelegramBotConfig
{
    public string TelegramBotToken { get; set; }
    public long? TelegramChatID{ get; set; }
    public double ResponseIntervalInSecond { get; set; } = 0.3;
    public bool LogRequestContent { get; set; } = false;
}
