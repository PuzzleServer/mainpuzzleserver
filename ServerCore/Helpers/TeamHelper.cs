using System.Linq;
using System.Threading.Tasks;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Utilities for working with teams
    /// </summary>
    static class TeamHelper
    {
        /// <summary>
        /// Helper for deleting teams that correctly deletes dependent objects
        /// </summary>
        /// <param name="team">Team to delete</param>
        public static async Task DeleteTeamAsync(PuzzleServerContext context, Team team)
        {
            var puzzleStates = from puzzleState in context.PuzzleStatePerTeam
                               where puzzleState.TeamID == team.ID
                               select puzzleState;
            context.PuzzleStatePerTeam.RemoveRange(puzzleStates);

            var hintStates = from hintState in context.HintStatePerTeam
                             where hintState.TeamID == team.ID
                             select hintState;
            context.HintStatePerTeam.RemoveRange(hintStates);

            var submissions = from submission in context.Submissions
                              where submission.Team == team
                              select submission;
            context.Submissions.RemoveRange(submissions);

            var annotations = from annotation in context.Annotations
                              where annotation.Team == team
                              select annotation;
            context.Annotations.RemoveRange(annotations);

            context.Teams.Remove(team);

            await context.SaveChangesAsync();
        }
    }
}
