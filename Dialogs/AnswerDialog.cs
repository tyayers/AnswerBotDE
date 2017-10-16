using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class AnswerDialog : LuisDialog<object>
    {
        public AnswerDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("WeatherNow")]
        public async Task WeatherNowIntent(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count > 0)
            {
                string city = result.Entities[0].Entity;
                string weatherResult = await GetCurrentWeather(result.Entities[0].Entity);
                Newtonsoft.Json.Linq.JObject jsonResult = JObject.Parse(weatherResult);

                string conditions = jsonResult["data"][0]["weather"]["description"].ToString();
                string temperature = jsonResult["data"][0]["temp"].ToString();
                await context.SayAsync($"Das aktuelle Wetter in {city} kann so zusammengefasst werden: {conditions}, {temperature}°.");

                context.Wait(MessageReceived);
            }
            else
            {
                await context.SayAsync($"Leider könnte ich nichts dazu finden!");
                context.Wait(MessageReceived);
            }
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("Wikipedia")]
        public async Task WikipediaIntent(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count > 0)
            {
                string wikiResult = await SearchWikipedia(result.Entities[0].Entity);

                Newtonsoft.Json.Linq.JArray jsonResult = JArray.Parse(wikiResult);
                JArray titleArray = (JArray)jsonResult[1];
                JArray descriptionArray = (JArray)jsonResult[2];
                JArray linkArray = (JArray)jsonResult[3];

                if (titleArray.Count > 0)
                {
                    string title = titleArray[0].ToString();
                    string description = descriptionArray[0].ToString();

                    await context.SayAsync($"Habe das gefunden: {title}", $"Found this: {title}");
                    await context.SayAsync($"{description}", $"{description}");

                    context.Wait(MessageReceived);
                }
            }
            else
            {
                await context.SayAsync($"Leider könnte ich nichts dazu finden!");
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

        private async Task<string> GetCurrentWeather(string city)
        {
            string result = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"http://api.weatherbit.io/v2.0/current?key=4a7c59bcaa6a464cb55e89eb5a81f7a8&city={city}&lang=de");

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