using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramGptBot.GPT;

public class GPTApiConfig
{
    public string GPTApiToken { get; set; }
    public string GPTModel { get; set; }
    public string Role { get; set; }
    public string ApiEndPoint { get; set; }
    public int SamplingTemperature { get; set; } = 1;
    public int NucleusSampling { get; set; } = 1;
    public int ChatCompletionChoicesForEachMessage { get; set; } = 1;
    public int? FurtherTokens { get; set; } = null;
    public int RequestRetryCount { get; set; } = 5;
    public int BetweenFailedRequestDelayInSecond { get; set; } = 5;
}
