using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class SinglePlayerPuzzleStateMapModel : EventSpecificPageModel
    {
        public List<PuzzleStats> PuzzleStatList { get; private set; }

        public List<PlayerStats> PlayerStatList { get; private set; }

        public StateStats[,] StateMap { get; private set; }

        public SinglePlayerPuzzleStateMapModel(PuzzleServerContext serverContext,
                        UserManager<IdentityUser> userManager)
            : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(string? groups)
        {
            // Get relevant SinglePlayerPuzzles
            List<Puzzle> puzzles;
            if (EventRole == EventRole.admin)
            {
                puzzles = await _context.Puzzles.Where(p => p.Event == Event)
                    .Where(p => p.IsForSinglePlayer)
                    .ToListAsync();
            }
            else
            {
                puzzles = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                    .Where(p => p.IsForSinglePlayer)
                    .ToListAsync();
            }

            // Gets all relevant single player puzzle states per player
            List<SinglePlayerPuzzleStatePerPlayer> puzzleStatesPerPlayer = await _context.SinglePlayerPuzzleStatePerPlayer
                .Where(puzzleState => puzzleState.Puzzle.Event == this.Event && puzzleState.Puzzle.IsForSinglePlayer)
                .ToListAsync();

            // Create the puzzle and player stats
            Dictionary<int, PuzzleStats> puzzleIdToPuzzleStatsMap = puzzles.ToDictionary(puzzle => puzzle.ID, puzzle => new PuzzleStats() { Puzzle = puzzle});
            var playerIdToPlayerStatsMap = new Dictionary<int, PlayerStats>();
            foreach (SinglePlayerPuzzleStatePerPlayer puzzleState in puzzleStatesPerPlayer)
            {
                if (!playerIdToPlayerStatsMap.ContainsKey(puzzleState.PlayerID))
                {
                    playerIdToPlayerStatsMap[puzzleState.PlayerID] = new PlayerStats { Player = puzzleState.Player };
                }

                if (puzzleState.SolvedTime != null)
                {
                    puzzleIdToPuzzleStatsMap[puzzleState.PuzzleID].SolveCount++;
                    playerIdToPlayerStatsMap[puzzleState.PlayerID].SolveCount++;
                    playerIdToPlayerStatsMap[puzzleState.PlayerID].Score += puzzleState.Puzzle.SolveValue;
                }
            }

            // Order both lists
            this.PuzzleStatList = puzzleIdToPuzzleStatsMap
                .Values
                .OrderBy(puzzleStat => puzzleStat.SolveCount)
                .ThenBy(puzzleStat => puzzleStat.Puzzle.Name)
                .ToList();

            this.PlayerStatList = playerIdToPlayerStatsMap
                .Values
                .OrderBy(playerStat => playerStat.Score)
                .ThenBy(playerStat => playerStat.SolveCount)
                .ThenBy(playerStat => playerStat.Player.Name)
                .ToList();

            // Create maps from each puzzle/player id to it's sorted index
            var index = 0;
            Dictionary<int, int> puzzleIdToIndexMap = this.PuzzleStatList.ToDictionary(puzzleStat => puzzleStat.Puzzle.ID, puzzleStat => index++);

            index = 0;
            Dictionary<int, int> playerIdToIndexMap = this.PlayerStatList.ToDictionary(playerStat => playerStat.Player.ID, playerStat => index++);

            // Create final stat matrix
            this.StateMap = new StateStats[this.PlayerStatList.Count, this.PuzzleStatList.Count];
            foreach (SinglePlayerPuzzleStatePerPlayer statePerPlayer in puzzleStatesPerPlayer)
            {
                int playerIndex = playerIdToIndexMap[statePerPlayer.PlayerID];
                int puzzleIndex = puzzleIdToIndexMap[statePerPlayer.Puzzle.ID];
                this.StateMap[playerIndex, puzzleIndex] = new StateStats()
                {
                    UnlockedTime = statePerPlayer.UnlockedTime,
                    SolvedTime = statePerPlayer.SolvedTime,
                    LockedOut = statePerPlayer.IsLockedOut
                };
            }

            return Page();
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle { get; set; }
            public int SolveCount { get; set; }
        }

        public class PlayerStats
        {
            public PuzzleUser Player { get; set; }
            public int SolveCount { get; set; }
            public int Score { get; set; }
        }

        public class StateStats
        {
            public static StateStats Default => new StateStats();

            public DateTime? UnlockedTime { get; set; }
            public DateTime? SolvedTime { get; set; }
            public bool LockedOut { get; set; }

            public string Classes
            {
                get
                {
                    string puzzleState = null;

                    if (LockedOut)
                    {
                        puzzleState = "email-mode";
                    }
                    else if (SolvedTime != null)
                    {
                        int minutes = (int)((DateTime.UtcNow - SolvedTime.Value).TotalMinutes);
                        puzzleState = minutes > 15 ? "solved-old" : "solved-recent";
                    }
                    else if (UnlockedTime == null)
                    {
                        puzzleState = "still-locked";
                    }
                    else
                    {
                        int minutes = (int)((DateTime.UtcNow - UnlockedTime.Value).TotalMinutes);
                        puzzleState = minutes > 15 ? "unlocked-old" : "unlocked-recent";
                    }

                    return puzzleState != null ? $"statecell {puzzleState}" : "statecell";
                }
            }
        }
    }
}
