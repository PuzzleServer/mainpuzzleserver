using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ServerMessages;

namespace ServerCore.Helpers
{
    public static class LiveEventHelper
    {
        private static IServiceProvider serviceProvider;

        public static async Task TriggerNotifications(PuzzleServerContext context, Event currentEvent, int timerWindow, IHubContext<ServerMessageHub> hubContext)
        {
            await PreEventReminder(context, currentEvent, timerWindow, hubContext);
            await EventOpeningReminder(context, currentEvent, hubContext);
            await EventClosingReminder(context, currentEvent, hubContext);
        }

        public static async Task<List<LiveEventSchedule>> GetTeamSchedule(PuzzleServerContext context, Event currentEvent, Team team)
        {
            var eventSchedule = await (from schedule in context.LiveEventsSchedule
                                       join liveEvent in context.LiveEvents on schedule.LiveEventId equals liveEvent.ID
                                       join puzzle in context.Puzzles on liveEvent.AssociatedPuzzleId equals puzzle.ID
                                       where puzzle.EventID == currentEvent.ID && schedule.TeamId == team.ID
                                       select schedule).ToListAsync();

            return eventSchedule;
        }

        /// <summary>
        /// Remind teams to go to a scheduled live event
        /// Notifies all teams that are within the current timer window
        /// </summary>
        public static async Task PreEventReminder(PuzzleServerContext context, Event currentEvent, int lengthOfTimerWindow, IHubContext<ServerMessageHub> hubContext)
        {
            var liveEvents = await GetLiveEventsForEvent(context, currentEvent, true, false);

            foreach (LiveEvent liveEvent in liveEvents)
            {
                string title = $"{liveEvent.Name} starts soon!";
                string firstReminderMessage = $"Your team's scheduled time for {liveEvent.Name} is in {liveEvent.FirstReminderOffset} minutes!";
                string lastReminderMessage = $"Your team's scheduled time for {liveEvent.Name} is in {liveEvent.LastReminderOffset} minutes! Time to head to {liveEvent.Location}";

                string puzzleLink = $"/{currentEvent.EventID}/play/Submissions/{liveEvent.AssociatedPuzzleId}";

                // Find first reminders
                // Logic: Event is in the future, the time until the event is less than the first reminder offset, and the team hasn't been notified yet
                var schedule = await context.LiveEventsSchedule.Where(s =>
                    (s.LiveEvent == liveEvent) &&
                    EF.Functions.DateDiffSecond(DateTime.UtcNow, s.StartTimeUtc) > 0 &&
                    EF.Functions.DateDiffSecond(DateTime.UtcNow, s.StartTimeUtc) < liveEvent.FirstReminderOffset.TotalSeconds &&
                    (s.LastNotifiedUtc.Year == 1 ||
                    EF.Functions.DateDiffSecond(s.LastNotifiedUtc, DateTime.UtcNow) > liveEvent.FirstReminderOffset.TotalSeconds)).ToListAsync();

                foreach (var teamSlot in schedule)
                {
                    await SendNotification(hubContext, title, firstReminderMessage, puzzleLink, team: teamSlot.Team);
                    teamSlot.LastNotifiedUtc = DateTime.UtcNow;
                }

                // Find second reminders
                // Note: The first reminder must be at least twice as long as the second reminder for this logic to be correct
                var lastSchedule = await context.LiveEventsSchedule.Where(s =>
                    (s.LiveEvent == liveEvent) &&
                    EF.Functions.DateDiffSecond(DateTime.UtcNow, s.StartTimeUtc) > 0 &&
                    EF.Functions.DateDiffSecond(DateTime.UtcNow, s.StartTimeUtc) < liveEvent.LastReminderOffset.TotalSeconds &&
                    (s.LastNotifiedUtc.Year == 1 ||
                    EF.Functions.DateDiffSecond(s.LastNotifiedUtc, DateTime.UtcNow) > liveEvent.LastReminderOffset.TotalSeconds)).ToListAsync();

                foreach (var teamSlot in lastSchedule)
                {
                    await SendNotification(hubContext, title, lastReminderMessage, puzzleLink, team: teamSlot.Team);
                    teamSlot.LastNotifiedUtc = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Blast all teams that an unscheduled event is opening soon
        /// Currently links to the puzzle page for that event, TODO: Link to a single live event page that shows the team's schedule
        /// </summary>
        public static async Task EventOpeningReminder(PuzzleServerContext context, Event currentEvent, IHubContext<ServerMessageHub> hubContext)
        {
            var allEvents = await (from liveEvent in context.LiveEvents
                                   join puzzle in context.Puzzles on liveEvent.AssociatedPuzzleId equals puzzle.ID
                                   where puzzle.EventID == currentEvent.ID
                                   select liveEvent).ToListAsync();

            var eventsOpening = from liveEvent in allEvents
                                where
                                liveEvent.EventStartTimeUtc > DateTime.UtcNow &&
                                liveEvent.EventStartTimeUtc - DateTime.UtcNow < liveEvent.OpeningReminderOffset &&
                                DateTime.UtcNow - liveEvent.LastNotifiedAllTeamsUtc > liveEvent.OpeningReminderOffset
                                select liveEvent;

            foreach (LiveEvent e in eventsOpening)
            {
                string puzzleLink = $"/{currentEvent.EventID}/play/Submissions/{e.AssociatedPuzzleId}";
                await SendNotification(hubContext, $"{e.Name} opening soon!", $"{e.Name} is opening in {e.OpeningReminderOffset} minutes! See you at {e.Location}!", puzzleLink, currentEvent);
                e.LastNotifiedAllTeamsUtc = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Blast all teams that an event is closing soon
        /// </summary>
        public static async Task EventClosingReminder(PuzzleServerContext context, Event currentEvent, IHubContext<ServerMessageHub> hubContext)
        {
            var allEvents = await (from liveEvent in context.LiveEvents
                                   join puzzle in context.Puzzles on liveEvent.AssociatedPuzzleId equals puzzle.ID
                                   where puzzle.EventID == currentEvent.ID
                                   select liveEvent).ToListAsync();

            var eventsClosing = from liveEvent in allEvents
                                where
                                liveEvent.EventEndTimeUtc > DateTime.UtcNow &&
                                liveEvent.EventEndTimeUtc - DateTime.UtcNow < liveEvent.ClosingReminderOffset &&
                                DateTime.UtcNow - liveEvent.LastNotifiedAllTeamsUtc > liveEvent.ClosingReminderOffset
                                select liveEvent;

            foreach (LiveEvent e in eventsClosing)
            {
                string puzzleLink = $"/{currentEvent.EventID}/play/Submissions/{e.AssociatedPuzzleId}";
                await SendNotification(hubContext, $"{e.Name} closing soon!", $"{e.Name} is closing in {e.ClosingReminderOffset} minutes! Go to {e.Location} right now if you haven't completed this event!", puzzleLink, currentEvent);
                e.LastNotifiedAllTeamsUtc = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Create LiveEventSchedule rows for each team and each scheduled live event
        /// This does not currently handle regeneration, in order to regenerate the schedule use the delete method, then run this again
        /// </summary>
        public static async Task GenerateScheduleForLiveEvents(PuzzleServerContext context, Event e, bool bigTeamsFirst = false, bool rotateTeams = false)
        {
            List<Team> teamList = await ShuffleTeams(context, e, bigTeamsFirst);
            int numScheduledEvents = await context.LiveEvents.Where(l => l.EventIsScheduled).CountAsync();
            int baseRotation = teamList.Count / numScheduledEvents;
            int rotation = baseRotation;

            // Get all of the scheduled live events for the current event
            var liveEvents = await GetLiveEventsForEvent(context, e, true, false);

            foreach (LiveEvent liveEvent in liveEvents)
            {
                DateTime currentSlot = liveEvent.EventStartTimeUtc;
                Queue<Team> teamQueue = new Queue<Team>(teamList);

                // If the plan is to rotate the team list to reduce double scheduling then do that here
                // The number of rotations is dependent on the number of scheduled live events
                if (rotateTeams)
                {
                    for (int i = 0; i <= rotation; i++)
                    {
                        Team current = teamQueue.Dequeue();
                        teamQueue.Enqueue(current);
                    }

                    rotation += baseRotation;
                }
                else
                {
                    // Reshuffle the team list for the next event - doesn't guarantee fairness or that events won't overlap but it's easy
                    teamList = await ShuffleTeams(context, e, bigTeamsFirst);
                }

                while (teamQueue.Count > 0)
                {
                    // For each instance starting at this time, assign the correct number of teams
                    for (int i = 0; i < liveEvent.NumberOfInstances; i++)
                    {
                        for (int j = 0; j < liveEvent.TeamsPerSlot; j++)
                        {
                            if (teamQueue.Count == 0)
                            {
                                break;
                            }

                            Team t = teamQueue.Dequeue();
                            LiveEventSchedule schedule = new LiveEventSchedule(liveEvent, t, currentSlot);
                            await context.LiveEventsSchedule.AddAsync(schedule);
                        }
                    }

                    currentSlot += liveEvent.TimePerSlot;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<List<Team>> ShuffleTeams(PuzzleServerContext context, Event e, bool bigTeamsFirst)
        {
            // Randomize the team list order so that teams don't get an advantage for early registration or names
            Random seed = new();

            // If the event isn't combining small teams, prioritize larger teams for the earlier time slots (but randomize within sizes)
            if (bigTeamsFirst)
            {
                List<Team> sortedTeamList = new List<Team>();

                var players = await (from teamMember in context.TeamMembers
                                     where teamMember.Team.EventID == e.ID
                                     select new { TeamId = teamMember.Team.ID }).ToListAsync();

                var teams = (from player in players
                             group player by player.TeamId into team
                             select new { TeamId = team.Key, Count = team.Count() });

                var sortedTeamIds = teams.OrderByDescending(team => team.Count).ThenBy(_ => seed.Next()).Select(team => team.TeamId).ToList();

                foreach (var teamId in sortedTeamIds)
                {
                    Team t = context.Teams.Where(t => t.ID == teamId && !t.IsRemoteTeam).FirstOrDefault();
                    if (t != null)
                    {
                        sortedTeamList.Add(t);
                    }
                }

                return sortedTeamList;
            }
            else
            {
                var teamList = await context.Teams.Where(t => t.Event == e && !t.IsRemoteTeam).ToListAsync();
                var sortedTeamList = teamList.OrderBy(_ => seed.Next());
                return sortedTeamList.ToList();
            }
        }

        /// <summary>
        /// Deletes all live event schedule slots for the given event. Hard delete, use with caution!
        /// </summary>
        /// <param name="context">Server context</param>
        /// <param name="eventId">Event</param>
        public static async Task DeleteLiveEventSchedule(PuzzleServerContext context, int eventId)
        {
            var eventSchedule = from schedule in context.LiveEventsSchedule
                                join liveEvent in context.LiveEvents on schedule.LiveEventId equals liveEvent.ID
                                join puzzle in context.Puzzles on liveEvent.AssociatedPuzzleId equals puzzle.ID
                                where puzzle.EventID == eventId
                                select schedule;

            await eventSchedule.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Triggers a notification for one or more teams
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="teamId">Defaults to -1, if not set it sends to all teams</param>
        private static async Task SendNotification(IHubContext<ServerMessageHub> hubContext, string title, string message, string puzzleUrl, Event e = null, Team team = null)
        {
            if (team == null)
            {
                await hubContext.SendNotification(e, title, message, linkUrl: puzzleUrl, isCritical: true);
            }
            else
            {
                await hubContext.SendNotification(team, title, message, linkUrl: puzzleUrl, isCritical: true);
            }
        }

        public static async Task<List<LiveEvent>> GetLiveEventsForEvent(PuzzleServerContext context, Event e, bool scheduledOnly, bool unscheduledOnly)
        {
            IQueryable<LiveEvent> liveEvents = from liveEvent in context.LiveEvents
                                               join puzzle in context.Puzzles on liveEvent.AssociatedPuzzleId equals puzzle.ID
                                               where puzzle.EventID == e.ID
                                               select liveEvent;

            if (scheduledOnly)
            {
                liveEvents = liveEvents.Where(l => l.EventIsScheduled);
            }

            if (unscheduledOnly)
            {
                liveEvents = liveEvents.Where(l => !l.EventIsScheduled);
            }

            return await liveEvents.ToListAsync();
        }

        public static async Task<string> ExportLiveEventScheduleToCsv(PuzzleServerContext context, Event e)
        {
            string csvFormattedOuput = String.Empty;

            // Get the set of scheduled times for the event
            List<LiveEvent> scheduledEvents = await GetLiveEventsForEvent(context, e, true, false);

            if (scheduledEvents.Count == 0)
            {
                return "";
            }

            scheduledEvents = scheduledEvents.OrderBy((liveEvent) => (liveEvent.ID)).ToList();
            List<LiveEventSchedule> combinedSchedule = new List<LiveEventSchedule>();

            StringBuilder byEventThenTime = new StringBuilder();

            foreach (LiveEvent liveEvent in scheduledEvents)
            {
                List<LiveEventSchedule> scheduledTimes = await (from eventSlot in context.LiveEventsSchedule
                                                                where eventSlot.LiveEventId == liveEvent.ID
                                                                orderby eventSlot.StartTimeUtc
                                                                select eventSlot).ToListAsync();

                // Add to the checkin list
                byEventThenTime.AppendLine(liveEvent.Name);

                foreach (LiveEventSchedule slot in scheduledTimes)
                {
                    byEventThenTime.AppendLine($"{slot.Team.Name},{slot.StartTimeUtc.ToLocalTime().ToShortTimeString()}");
                }

                combinedSchedule.AddRange(scheduledTimes);
            }

            StringBuilder byTeam = new StringBuilder();
            combinedSchedule = combinedSchedule.OrderBy((slot) => (slot.TeamId)).ThenBy((slot) => (slot.LiveEventId)).ToList();

            // Set up the header line - the events are ordered by id above so the order is consistent here
            byTeam.Append("Team Name,");

            foreach (LiveEvent liveEvent in scheduledEvents)
            {
                byTeam.Append(liveEvent.Name + ",");
            }

            int currentTeamId = -10;

            foreach (LiveEventSchedule slot in combinedSchedule)
            {
                if (slot.TeamId != currentTeamId)
                {
                    // End the line for the current team
                    byTeam.AppendLine();

                    // Set the currentTeamId to the next team & put their team name at the start of the line
                    currentTeamId = slot.TeamId;
                    byTeam.Append(slot.Team.Name + ",");
                }

                // Add event start times ordered by event ID
                byTeam.Append(slot.StartTimeUtc.ToLocalTime().ToShortTimeString() + ",");
            }

            csvFormattedOuput = byEventThenTime.ToString() + byTeam.ToString();

            return csvFormattedOuput;
        }
    }
}
