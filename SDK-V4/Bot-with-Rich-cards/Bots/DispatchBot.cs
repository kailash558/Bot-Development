using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class DispatchBot : ActivityHandler
    {
        private ILogger<DispatchBot> _logger;
        private IBotServices _botServices;

        public DispatchBot(IBotServices botServices, ILogger<DispatchBot> logger)
        {
            _logger = logger;
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(turnContext, cancellationToken);
            
            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();
            
            // Next, we call the dispatcher with the top intent.
            await DispatchToTopIntentAsync(turnContext, topIntent.intent, recognizerResult, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string WelcomeText = "How can I help you today?";

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome {member.Name}. {WelcomeText}"), cancellationToken);
                }
            }
        }

        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "l_luis":
                    await ProcessluisAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                    break;
        
                case "q_sample-qna":
                    await ProcessSampleQnAAsync(turnContext, cancellationToken);
                    break;
                default:
                    _logger.LogInformation($"Dispatch unrecognized intent: {intent}.");
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    break;
            }
        }

        private async Task ProcessluisAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessluisAsync");

            var result = luisResult.ConnectedServiceResult;
            var topIntent = result.TopScoringIntent.Intent; 
            
            await turnContext.SendActivityAsync(MessageFactory.Text($"Luis top intent {topIntent}."), cancellationToken);
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Luis intents detected:\n\n{string.Join("\n\n", result.Intents.Select(i => i.Intent))}"), cancellationToken);
            if (luisResult.Entities.Count > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Luis entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);
            }
        }

        private async Task ProcessSampleQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessSampleQnAAsync");

            var results = await _botServices.SampleQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                var ans = results.First().Answer;
                //var answer = MessageFactory.Text(results.First().Answer);
                //await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
                //var ans = answer.ToString();
                //var attachment = GetHeroCard(ans);
                //var reply = MessageFactory.Attachment(attachment);
                string[] qnaAnswerData = ans.Split(';');
                int dataSize = qnaAnswerData.Length;
                if (dataSize > 1 && dataSize <= 6)
                {
                    var attachment = GetHeroCard(ans);
                    var reply = MessageFactory.Attachment(attachment);
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
                }

                
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }

        private static Attachment GetHeroCard(string ans)
        {
            string[] qnaAnswerData = ans.Split(';');
            string title = qnaAnswerData[0];
            string description = qnaAnswerData[1];
            string url = qnaAnswerData[2];
            string imageURL = qnaAnswerData[3];
            HeroCard card = new HeroCard
            {
                Title = title,
                Subtitle = description,
            };
            card.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
            };

            card.Images = new List<CardImage>
            {
                new CardImage(url = imageURL)
            };
            return card.ToAttachment();

        }
    }
}
