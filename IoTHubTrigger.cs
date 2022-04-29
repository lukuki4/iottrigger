using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace workshop_2
{

    public class TemperatureItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public double BodyTemperature { get; set; }
        public bool Light { get; set; }
        public bool StreetlightStatus { get; set; }
        public int Fanspeed { get; set; }
        public bool electricityMonitor { get; set; }
        public bool ambientLight { get; set; }
        public double Heartrate { get; set; }
    }

    public class fit
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public double Body_Temperature { get; set; }
        // public double Humidity { get; set; }
    }

    public static class myIoTHubTrigger
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("myIoTHubTrigger")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "IotSimulationData",
                                 collectionName: "Simulations",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out TemperatureItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            var jsonBody = Encoding.UTF8.GetString(message.Body);
            dynamic data = JsonConvert.DeserializeObject(jsonBody);
            double Body_Temperature = data.Body_Temperature;
            double Heartrate = data.Heartrate;
            bool light = data.lightsOn;
            int fanspeed = data.fanSpeed;
            bool streetlightStatus = data.streetlightStatus;
            bool ambientLight = data.ambientLight;
            bool electricityMonitor = data.electricityMonitor;

            output = new TemperatureItem
            {
                BodyTemperature = Body_Temperature,
                Heartrate = Heartrate,
                Light = light,
                Fanspeed = fanspeed,
                StreetlightStatus = streetlightStatus,
                electricityMonitor = electricityMonitor,
                ambientLight = ambientLight
            };
        }

        [FunctionName("myIoTHubOutput")]
        public static IActionResult myIoTHubTriggers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperature/")] HttpRequest req,
        [CosmosDB(databaseName: "IotSimulationData",
                  collectionName: "Simulations",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT * FROM simulations")] IEnumerable<TemperatureItem> temperatureItem,
                  ILogger log)
        {
            // Create a new list of filtered data to be returned
            List<fit> filtered = new List<fit>();
            //loopp thru data
            foreach (TemperatureItem elem in temperatureItem)
            {
                // create new filtered obj
                if (elem.BodyTemperature > 30)
                {
                    fit fe = new fit();
                    // set params from query
                    fe.Body_Temperature = elem.BodyTemperature;
                    fe.Id = elem.Id;
                    // append to new list
                    filtered.Add(fe);

                }
            }
            // Return our filtered list
            return new OkObjectResult(filtered);
        }

    }
}

