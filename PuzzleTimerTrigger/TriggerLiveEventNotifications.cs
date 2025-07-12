using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace PuzzleTimerTrigger
{
    public class TriggerLiveEventNotifications
    {
        static HttpClient HttpClient { get; } = new HttpClient();
        [FunctionName("TriggerLiveEventNotifications")]
        public void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            const string eventId = "pd2025";
            HttpClient.PostAsync($"https://puzzlehunt.azurewebsites.net/api/puzzleapi/liveevent/triggernotifications?eventId={eventId}&timerWindow=10", null).Wait();
        }
    }
}
