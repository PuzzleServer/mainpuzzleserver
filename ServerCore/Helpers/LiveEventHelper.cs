using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Migrations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using ServerCore.ServerMessages;

namespace ServerCore.Helpers
{
    public static class LiveEventHelper
    {
        private static IServiceProvider serviceProvider;

        public static async Task TriggerNotifications(PuzzleServerContext context, Event currentEvent, int timerWindow)
        {
            await PreEventReminder(context, currentEvent, timerWindow);
            await EventOpeningReminder(context, currentEvent);
            await EventClosingReminder(context, currentEvent);
        }

        /// <summary>
        /// Remind teams to go to a scheduled live event
        /// Notifies all teams that are within the current timer window
        /// </summary>
        public static async Task PreEventReminder(PuzzleServerContext context, Event currentEvent, int lengthOfTimerWindow)
        {
            var liveEvents = GetLiveEventsForEvent(context, currentEvent, true, false);

            foreach (LiveEvent liveEvent in liveEvents)
            {
                string title = $"{liveEvent.Name} starts soon!";
                string firstReminderMessage = $"Your team's scheduled time for {liveEvent.Name} is in {liveEvent.FirstReminderOffset} minutes!";
                string lastReminderMessage = $"Your team's scheduled time for {liveEvent.Name} is in {liveEvent.LastReminderOffset} minutes! Time to head to {liveEvent.Location}";

                // Find first reminders
                var schedule = context.LiveEventsSchedule.Where(s =>
                    (s.LiveEvent == liveEvent) &&
                    (s.StartTimeUtc - DateTime.UtcNow) < liveEvent.FirstReminderOffset &&
                    (s.LastNotifiedUtc - DateTime.UtcNow) > liveEvent.FirstReminderOffset);

                foreach (var teamSlot in schedule)
                {
                    await SendNotifiction(title, firstReminderMessage, liveEvent.AssociatedPuzzle.CustomURL, team: teamSlot.Team);
                    teamSlot.LastNotifiedUtc = DateTime.UtcNow;
                }

                // Find second reminders
                var lastSchedule = context.LiveEventsSchedule.Where(s =>
                    (s.LiveEvent == liveEvent) &&
                    (s.StartTimeUtc - DateTime.UtcNow) < liveEvent.LastReminderOffset &&
                    (s.LastNotifiedUtc - DateTime.UtcNow) > liveEvent.LastReminderOffset);

                foreach (var teamSlot in lastSchedule)
                {
                    await SendNotifiction(title, lastReminderMessage, liveEvent.AssociatedPuzzle.CustomURL, team: teamSlot.Team);
                    teamSlot.LastNotifiedUtc = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Blast all teams that an unscheduled event is opening soon
        /// Currently links to the puzzle page for that event, TODO: Link to a single live event page that shows the team's schedule
        /// </summary>
        public static async Task EventOpeningReminder(PuzzleServerContext context, Event currentEvent)
        {
            IQueryable<LiveEvent> eventsOpening = context.LiveEvents.Where(l =>
            (l.EventStartTimeUtc - DateTime.UtcNow) < l.OpeningReminderOffset &&
            (l.LastNotifiedAllTeamsUtc - DateTime.UtcNow) > l.OpeningReminderOffset);

            foreach (LiveEvent e in eventsOpening)
            {
                await SendNotifiction($"{e.Name} opening soon!", $"{e.Name} is opening in {e.OpeningReminderOffset} minutes! See you at {e.Location}!", e.AssociatedPuzzle.CustomURL, currentEvent);
                e.LastNotifiedAllTeamsUtc = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Blast all teams that an event is closing soon
        /// </summary>
        public static async Task EventClosingReminder(PuzzleServerContext context, Event currentEvent)
        {
            IQueryable<LiveEvent> eventsOpening = context.LiveEvents.Where(l =>
                (l.EventStartTimeUtc - DateTime.UtcNow) < l.ClosingReminderOffset &&
                (l.LastNotifiedAllTeamsUtc - DateTime.UtcNow) > l.ClosingReminderOffset);

            foreach (LiveEvent e in eventsOpening)
            {
                await SendNotifiction($"{e.Name} closing soon!", $"{e.Name} is closing in {e.ClosingReminderOffset} minutes! Go to {e.Location} right now if you haven't completed this event!", e.AssociatedPuzzle.CustomURL, currentEvent);
                e.LastNotifiedAllTeamsUtc = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Create LiveEventSchedule rows for each team and each scheduled live event
        /// This does not currently handle regeneration, in order to regenerate the schedule use the delete method, then run this again
        /// </summary>
        private static async Task GenerateScheduleForLiveEvents(PuzzleServerContext context, Event e)
        {
            // Randomize the team list order so that teams don't get an advantage for early registration or names
            IQueryable<Team> teamList = context.Teams.Where(t => t.Event == e);
            Random seed = new();
            teamList.OrderBy(_ => seed.Next());

            // Get all of the scheduled live events for the current event
            var liveEvents = GetLiveEventsForEvent(context, e, true, false);

            foreach(LiveEvent liveEvent in liveEvents)
            {
                DateTime currentSlot = liveEvent.EventStartTimeUtc;
                Queue<Team> teamQueue = new Queue<Team>(teamList);

                while (teamQueue.Count > 0)
                {
                    // For each instance starting at this time, assign the correct number of teams
                    for (int i = 0; i < liveEvent.NumberOfInstances; i++)
                    {
                        for (int j = 0; j < liveEvent.TeamsPerSlot; j++)
                        {
                            Team t = teamQueue.Dequeue();
                            LiveEventSchedule schedule = new LiveEventSchedule(e, liveEvent, t, currentSlot);
                            await context.LiveEventsSchedule.AddAsync(schedule);
                        }
                    }

                    currentSlot += liveEvent.TimePerSlot;
                }

                // Shuffle the team list - doesn't guarantee fairness or that events won't overlap but it's easy
                teamList.OrderBy(_ => seed.Next());
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all live event schedule slots for the given event. Hard delete, use with caution!
        /// </summary>
        /// <param name="context">Server context</param>
        /// <param name="e">Event</param>
        public static async Task DeleteLiveEventSchedule(PuzzleServerContext context, Event e)
        {
            var eventSchedule = context.LiveEventsSchedule.Where(s => s.Event == e);
            await eventSchedule.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Triggers a notification for one or more teams
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="teamId">Defaults to -1, if not set it sends to all teams</param>
        private static async Task SendNotifiction(string title, string message, string puzzleUrl, Event e = null, Team team = null)
        {
            if (team == null)
            {
                await serviceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(e, title, message, puzzleUrl);
            }
            else
            {
                await serviceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(team, title, message, puzzleUrl);
            }
        }

        private static List<LiveEvent> GetLiveEventsForEvent(PuzzleServerContext context, Event e, bool scheduledOnly, bool unscheduledOnly)
        {
            IQueryable<LiveEvent> liveEvents = context.LiveEvents.Where(l => (l.AssociatedEvent == e));

            if (scheduledOnly)
            {
                liveEvents = liveEvents.Where(l => l.EventIsScheduled);
            }

            if (unscheduledOnly) 
            {
                liveEvents = liveEvents.Where(l => !l.EventIsScheduled);
            }

            return liveEvents.ToList();
        }
    }
}
