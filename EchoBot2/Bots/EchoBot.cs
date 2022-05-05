// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.13.2

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoBot2.Bots
{
    public class EchoBot : ActivityHandler
    {
        static string endpoint = "https://southeastasia.api.cognitive.microsoft.com/";
        static string subscriptionKey = "______5240bab67f2cb________";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo : {turnContext.Activity.Text}";

            //if (turnContext.Activity.Attachments.Count > 0)
            //{
            //    var attachmentUrl = turnContext.Activity.Attachments[0].ContentUrl;
            //    var httpClient = new HttpClient();
            //    var attachmentData =
            //        await httpClient.GetByteArrayAsync(attachmentUrl);

            //    var ret = ComputerVisionHelper.MakeRequest(endpoint, subscriptionKey, attachmentData);
            //     replyText = $"圖片內容是: {ret.description.captions[0]}";
            //}

            if (turnContext.Activity.Attachments != null && turnContext.Activity.Attachments.Count > 0)
            {
                var attachmentUrl = turnContext.Activity.Attachments[0].ContentUrl;
                var httpClient = new HttpClient();
                var attachmentData =
                    await httpClient.GetByteArrayAsync(attachmentUrl);

                var ret = ComputerVisionHelper.ProcessImage(endpoint, subscriptionKey, attachmentData);
                replyText = $"圖片內容是 -->  {ret.Text}  {System.Environment.NewLine} URL: {ret.ImageURL}";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
