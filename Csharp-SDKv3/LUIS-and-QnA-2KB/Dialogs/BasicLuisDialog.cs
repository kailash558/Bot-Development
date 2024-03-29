
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System.Text;

public class Metadata
{
    public string name { get; set; }
    public string value { get; set; }
}

public class Answer
{
    public IList<string> questions { get; set; }
    public string answer { get; set; }
    public double score { get; set; }
    public int id { get; set; }
    public string source { get; set; }
    public IList<object> keywords { get; set; }
    public IList<Metadata> metadata { get; set; }
}

public class QnAAnswer
{
    public IList<Answer> answers { get; set; }
}

[Serializable]
public class QnAMakerService
{
    private string qnaServiceHostName;
    private string knowledgeBaseId;
    private string endpointKey;

    public QnAMakerService(string hostName, string kbId, string endpointkey)
    {
        qnaServiceHostName = hostName;
        knowledgeBaseId = kbId;
        endpointKey = endpointkey;

    }
    async Task<string> Post(string uri, string body)
    {
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(uri);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", "EndpointKey " + endpointKey);

            var response = await client.SendAsync(request);
            return  await response.Content.ReadAsStringAsync();
        }
    }
    public async Task<string> GetAnswer(string question)
    {
        string uri = qnaServiceHostName + "/qnamaker/knowledgebases/" + knowledgeBaseId + "/generateAnswer";
        string questionJSON = "{\"question\": \"" + question.Replace("\"","'") +  "\"}";

        var response = await Post(uri, questionJSON);

        var answers = JsonConvert.DeserializeObject<QnAAnswer>(response);
        if (answers.answers.Count > 0)
        {
            return answers.answers[0].answer;
        }
        else
        {
            return "No good match found.";
        }
    }
}



namespace Microsoft.Bot.Sample.LuisBot
{

    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        //static string LUIS_appId = "Luis-app-id";
   // static string LUIS_apiKey = "Luis-API-key";
    //static string LUIS_hostRegion = "westus.api.cognitive.microsoft.com";

    // QnA Maker global settings
    // assumes all KBs are created with same Azure service
        static string qnamaker_endpointKey = "KB-endpoint-key";
        static string qnamaker_endpointDomain = "myfaqbot";
    
        // QnA Maker Chitchat Knowledge base
        static string chitChat_kbID = "KB-1-ID";
    
        // QnA Maker Azure - Bot Development Knowledge base
        static string azure_kbID = "KB-2-ID";
    
        // Instantiate the knowledge bases
        public QnAMakerService chitChatQnAService = new QnAMakerService("https://" + qnamaker_endpointDomain + ".azurewebsites.net", chitChat_kbID, qnamaker_endpointKey);
        public QnAMakerService azureQnAService = new QnAMakerService("https://" + qnamaker_endpointDomain + ".azurewebsites.net", azure_kbID, qnamaker_endpointKey);
    
    
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
            {
            }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            HttpClient client = new HttpClient();
            await this.ShowLuisResult(context, result);

        }
        
        // azureBot intent
        [LuisIntent("azureBot")]
        public async Task azureBotIntent(IDialogContext context, LuisResult result)
        {
            var qnaMakerAnswer = await azureQnAService.GetAnswer(result.Query);
            await context.PostAsync($"{qnaMakerAnswer}");
            context.Wait(MessageReceived);
        }
        //Greeting Intent
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result){
            var qnaMakerAnswer = await chitChatQnAService.GetAnswer(result.Query);
            await context.PostAsync($"{qnaMakerAnswer}");
            context.Wait(MessageReceived);
        }
        
        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result){
            var qnaMakerAnswer = await azureQnAService.GetAnswer(result.Query);
            await context.PostAsync($"{qnaMakerAnswer}");
            context.Wait(MessageReceived);
        }
        
        //cloudService intent
        [LuisIntent("cloudService")]
        public async Task cloudServiceIntent(IDialogContext context, LuisResult result){
            var qnaMakerAnswer = await azureQnAService.GetAnswer(result.Query);
            await context.PostAsync($"{qnaMakerAnswer}");
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("No good answer found in KB");
            context.Wait(MessageReceived);
        }
    }
}
