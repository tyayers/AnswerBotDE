using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class WikipediaDialog : LuisDialog<object>
    {
        public WikipediaDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("Wikipedia")]
        public async Task WikipediaIntent(IDialogContext context, LuisResult result)
        {
			string wikiResult = await SearchWikipedia(message.Text);
			
	        Newtonsoft.Json.Linq.JArray jsonResult = JArray.Parse(wikiResult);
            JArray titleArray = (JArray)jsonResult[1];
            JArray descriptionArray = (JArray)jsonResult[2];
            JArray linkArray = (JArray)jsonResult[3];
            
            if (titleArray.Count > 0) {
                string title = titleArray[0].ToString();
                string description = descriptionArray[0].ToString();
                
                await context.PostAsync($"Found this on Wikipedia: {title}"); 
                await context.PostAsync($"{description}"); 
                                
                context.Wait(MessageReceived);                
            }
        }
		
        private async Task<string> SearchWikipedia(string message)
        {
            string result = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://de.wikipedia.org/w/api.php?action=opensearch&format=json&search=" + message);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("");
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }		
    }
}