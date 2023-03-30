using System.Net.Http.Headers;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace TelegramGptBot.GPT;

public class GPTContext
{
    private readonly GPTApiConfig _apiConfig;
    private HttpClient httpClient = new HttpClient();
    private AsyncRetryPolicy<HttpResponseMessage> retryPolicy;
    public GPTContext(GPTApiConfig apiConfig) {
        _apiConfig = apiConfig;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.GPTApiToken);
        retryPolicy = CreateRetryPolicy();
    }
    private AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy()
    {

        return Policy.Handle<HttpRequestException>()
             .OrResult<HttpResponseMessage>(r => r.IsSuccessStatusCode)
             .WaitAndRetryAsync(_apiConfig.RequestRetryCount, (_) => TimeSpan.FromSeconds(_apiConfig.BetweenFailedRequestDelayInSecond), (exception, timeSpan, retryCount, context) =>
             {
                 if (exception.Exception is not null)
                 {
                     Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd/HH/mm/ss")} - GPT Request Exception: {exception.Exception.Message}");
                 }
                
             });            
    }
    private async  Task<HttpResponseMessage> ExcuteWithPolicy(Func<Task<HttpResponseMessage>> HttpRequestFunc) {

        return await retryPolicy.ExecuteAsync(HttpRequestFunc);
    }
    public async Task<Result<String,Exception>> GetAnswer(string Question)
    {
        var messageContent = BuildRequestBody(Question);
        var gptResponse = await ExcuteWithPolicy(async () => await httpClient.PostAsync(_apiConfig.ApiEndPoint, messageContent));
        var response = await gptResponse.Content.ReadAsStringAsync();
        if (!gptResponse.IsSuccessStatusCode)
        {
            return new HttpRequestException(response);
        }
        var responseContent = await gptResponse.Content.ReadAsStringAsync();
        var deserializeContent = JsonConvert.DeserializeObject<GptResponse>(responseContent);
        if (deserializeContent.choices.FirstOrDefault() is null)
        {
            return new Exception("No Answer in Response");
        }
        return deserializeContent.choices.First().message.content;
    }
    public StringContent BuildRequestBody(string Question) {
        var serializedMessage = JsonConvert.SerializeObject(new
        {
            model = _apiConfig.GPTModel,
            messages = new List<dynamic>
            {   new 
                {
                    role = _apiConfig.Role,
                    content = Question 
                }
            },
            temperature = _apiConfig.SamplingTemperature,
            top_p = _apiConfig.NucleusSampling,
            n = _apiConfig.ChatCompletionChoicesForEachMessage,
            stop = _apiConfig.FurtherTokens,
        });
        return new StringContent(serializedMessage,MediaTypeHeaderValue.Parse("application/json"));        
    }
}
