# NLP with Dispatch

Bot Framework v4 NLP with Dispatch bot sample

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a bot that relies on multiple [LUIS.ai](https://www.luis.ai) and [QnAMaker.ai](https://www.qnamaker.ai) models for natural language processing (NLP).

Use the Dispatch model in cases when:

- Your bot consists of multiple language modules (LUIS + QnA) and you need assistance in routing user's utterances to these modules in order to integrate the different modules into your bot.
- Evaluate quality of intents classification of a single LUIS model.
- Create a text classification model from text files.


### Use Dispatch with Mulitple LUIS and QnA Models

To learn how to configure Dispatch with multiple LUIS models and QnA Maker services, refer to the steps found [here](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-tutorial-dispatch?view=azure-bot-service-4.0).

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Dispatch

Once you are confortable with the concepts presented in this sample, you may want to configure Dispath using a CLI tool.  [Dispatch CLI](https://github.com/Microsoft/botbuilder-tools/tree/master/packages/Dispatch) is a CLI tool that enables you to create a dispatch NLP model across the different LUIS applications and/ or QnA Maker Knowledge Bases you have for your bot.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.
