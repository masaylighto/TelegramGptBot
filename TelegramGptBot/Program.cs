using Microsoft.Extensions.Configuration;
using TelegramGptBot.GPT;
using TelegramGptBot.Telegram;
//Load Configuration
var configuration = new ConfigurationBuilder().AddJsonFile("AppSettings.json").AddEnvironmentVariables().Build();

//Create Config class
GPTApiConfig? gptApiConfig= new GPTApiConfig();
TelegramBotConfig telegramBotConfig= new TelegramBotConfig();
configuration.GetRequiredSection("GptApiConfig").Bind(gptApiConfig); // bind config from json file
configuration.GetRequiredSection("TelegramBot").Bind(telegramBotConfig);
configuration.Bind(gptApiConfig); // bind from environment variable
configuration.Bind(telegramBotConfig);// bind from environment variable



//ensure none of the config class is null before proceeds 
if (gptApiConfig is null)
{
    Console.WriteLine("Gpt Api Configuration is not provided");
    return;
}
if (telegramBotConfig is null)
{
    Console.WriteLine("Telegram Bot Configuration is not provided in the environment variable");
    return;
}



//Start the bot 
try
{

    GPTContext gptContext = new GPTContext(gptApiConfig);
    TelegramContext telegramContext = new TelegramContext(telegramBotConfig)
    {
        MessageProccessor =  gptContext.GetAnswer
    };
    telegramContext.StartMessageMonitoring();
}
catch(Exception ex) 
{     
    Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - {ex.Message}");
}
while (true) { 
Thread.Sleep(1000);
};