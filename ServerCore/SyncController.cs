using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages
{
    public class SyncHelper
    {
        /// <summary>
        ///   The AnnotationRequest class represents a request to store an annotation with a certain key and contents.
        /// </summary>
        public class AnnotationRequest
        {
            public int key;
            public string contents;
        }

        /// <summary>
        ///   The DecodedSyncRequest class represents a request to perform a sync, decoded from the format posted
        ///   by Javascript.
        /// </summary>
        public class DecodedSyncRequest
        {
            /// <summary>
            ///   The list of puzzled IDs to check whether they're solved yet.  Could be null.
            /// </summary>

            public readonly List<int> QueryPuzzleIds;

            /// <summary>
            ///   If MinSolveCount isn't null, the requester wants any pieces the user has earned
            ///   corresponding to a solve count greater than or equal to MinSolveCount.  For
            ///   instance, if this is the first time the requester is asking for information, it'll
            ///   set this to 0; if this is a subsequent time, the requester will set it to
            ///   MaxSolveCount+1, where MaxSolveCount was the maximum solve count they were told
            ///   they received last time.  If this is an "annotation upload only" sync, then
            ///   MinSolveCount will be null, meaning "don't bother computing what pieces I've
            ///   earned, because I don't want any right now".
            /// </summary>
            public readonly int? MinSolveCount;

            /// <summary>
            ///   If AnnotationRequests isn't null, the requester wants to upload all the
            ///   annotations described by these requests.
            /// </summary>
            public readonly List<AnnotationRequest> AnnotationRequests;

            /// <summary>
            ///   If LastSyncTime isn't null, the requester wants all the annotations that the team
            ///   has created for this puzzle since LastSyncTime.  If LastSyncTime is null, the
            ///   requester wants all annotations that the team has created for this puzzle, no
            ///   matter when they were created.
            /// </summary>
            public readonly DateTime? LastSyncTime;

            /// <summary>
            ///   If an error was encountered while processing the raw parameters passed to the sync
            ///   controller, this will be a non-null string that can be returned to the user to
            ///   give a meaningful error message.  It's intended that if the request comes from a
            ///   page we created, no error will ever be raised.
            /// </summary>
            public readonly string DecodingError;

            public DecodedSyncRequest(List<int> query_puzzle_ids, int? min_solve_count, string annotations, string last_sync_time,
                                      int maxAnnotationKey, ref SyncResponse response)
            {
                // Initialize the error to null, and add to it as we find errors.

                DecodingError = null;

                // The query_puzzle_ids and min_solve_count fields are already perfectly fine, so just copy them.

                QueryPuzzleIds = query_puzzle_ids;
                MinSolveCount = min_solve_count;

                // The last_sync_time field is a JSON-converted DateTime.  Or at least, it's supposed to be;
                // we need to check.  Note that this string is one that we (the server) encoded.  We do DateTime
                // encoding and decoding on the server so as not to worry about different browsers encoding
                // DateTimes differently.

                if (last_sync_time == null) {
                    LastSyncTime = null;
                }
                else {
                    try {
                        LastSyncTime = JsonConvert.DeserializeObject<DateTime>(last_sync_time);
                    }
                    catch (JsonException) {
                        LastSyncTime = null;
                    }
                }

                var my_annotations = new List<AnnotationRequest> { new AnnotationRequest { key = 7, contents = "foo" } };
                var my_serialization = JsonConvert.SerializeObject(my_annotations);

                // The annotations field is a JSON-converted list of objects, each with an integer key and
                // string contents.  Or at least, it's supposed to be; we have to check it carefully.

                AnnotationRequests = null;
                if (annotations != null) {
                    try {
                        // First, decode it into a list using the JSON deserializer.
                        AnnotationRequests = JsonConvert.DeserializeObject<List<AnnotationRequest>>(annotations);
                    }
                    catch (JsonException) {
                        response.AddError("Could not deserialize annotations list");
                        AnnotationRequests = null;
                    }

                    // If that succeeded, check each of the lements for validity.

                    if (AnnotationRequests != null) {
                        foreach (var annotation in AnnotationRequests) {
                            if (annotation.key < 1) {
                                response.AddError("Found non-positive key in annotation");
                                AnnotationRequests = null;
                                break;
                            }
                            if (annotation.key > maxAnnotationKey) {
                                response.AddError("Found too-high in annotation");
                                AnnotationRequests = null;
                                break;
                            }
                            if (annotation.contents.Length > 255) {
                                response.AddError("Found contents in annotation longer than 255 bytes");
                                AnnotationRequests = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   A SyncResponse gathers all the data that needs to be sent back to the requester of a
        ///   sync, and supplies it as an IActionResult when the caller calls GetResult().
        /// </summary>
        public class SyncResponse
        {
            private string currentError;
            private dynamic response;

            public SyncResponse()
            {
                currentError = null;
                response = new ExpandoObject();
            }

            public bool HasError { get { return currentError != null; } }

            public void AddError(string newError)
            {
                if (newError == null) {
                    return;
                }
                else if (currentError == null) {
                    currentError = newError;
                }
                else {
                    currentError = currentError + " " + newError;
                }
            }

            public void SetSiteVersion(string siteVersion)
            {
                response.site_version = siteVersion;
            }

            public void SetMinAndMaxSolveCountAndPieces(int minSolveCount, int maxSolveCount, List<Piece> pieces)
            {
                response.min_solve_count = minSolveCount;
                response.max_solve_count = maxSolveCount;
                response.tokens = pieces.Select(piece => new { solve_count = piece.ProgressLevel, contents = piece.Contents } );
            }

            public void SetSolvedPuzzles(List<int> solvedPuzzles)
            {
                response.solved_puzzles = solvedPuzzles;
            }

            public void SetSyncTimeAndAnnotations(DateTime syncTime, List<Annotation> annotations)
            {
                response.sync_time = JsonConvert.SerializeObject(syncTime);
                response.annotations = annotations.Select(a => new { key = a.Key, contents = a.Contents, version = a.Version });
            }

            public dynamic GetResult()
            {
                if (currentError != null) {
                    return new { error = currentError };
                }
                else {
                    return response;
                }
            }
        }

        private readonly PuzzleServerContext context;

        public SyncHelper(PuzzleServerContext i_context)
        {
            context = i_context;
        }

        /// <summary>
        ///   Sometimes, an author finds a bug in a puzzle page and would like that puzzle page to
        ///   reload itself to get the changes.  If that puzzle page periodically invokes the sync
        ///   API, the sync API provides a way to achieve this.  The sync response indicates the
        ///   current "version" of the puzzle, essentially telling the requesting page to reload
        ///   itself if it's different from the last time it found out the version.
        ///
        ///   So, the point of GetSiteVersion is to return a string that changes with every update
        ///   to the page of the corresponding puzzle.
        /// </summary>
        private string GetSiteVersion(Puzzle puzzle)
        {
            // if (puzzle.ID == 8) { return "2" };   // use a line like this to indicate a puzzle-specific version
            // if (puzzle.ID == 97) { return "3" };  // use a line like this to indicate a puzzle-specific version
            return "1";
        }

        /// <summary>
        ///   This routine handles sync request aspects that require fetching a list of all the puzzles the team
        ///   has solved.
        /// </summary>
        private async Task HandleSyncAspectsRequiringListOfSolvedPuzzles(DecodedSyncRequest request, SyncResponse response,
                                                                         string groupExcludedFromSolveCount, int teamId, int eventId)
        {
            // If the requester isn't asking for pieces (by setting MinSolveCount to null), and isn't asking about
            // whether any puzzle IDs are solved, we can save time by not querying the list of solved puzzles.

            if (request.MinSolveCount == null && (request.QueryPuzzleIds == null || request.QueryPuzzleIds.Count == 0)) {
                return;
            }

            // If the request is asking which of the puzzle IDs in request.QueryPuzzleIds has been
            // solved, create a HashSet so that we can quickly look up if an ID is in that list.

            HashSet<int> queryPuzzleIdSet = null;
            if (request.QueryPuzzleIds != null) {
                queryPuzzleIdSet = new HashSet<int>();
                foreach (var queryPuzzleId in request.QueryPuzzleIds) {
                    queryPuzzleIdSet.Add(queryPuzzleId);
                }
            }

            // Get a list of all the puzzles this team has solved from this event.

            List<Puzzle> solves = await (from state in context.PuzzleStatePerTeam
                                         where state.TeamID == teamId && state.SolvedTime != null
                                         select state.Puzzle).ToListAsync();

            int maxSolveCount = 0;
            List<int> solvedPuzzles = new List<int>();
            foreach (var solvedPuzzle in solves) {
                // I'm pretty sure the query above will never return any puzzles that aren't
                // from the current event.  But, just in case, out of paranoia, we check
                // for them and don't count them.

                if (solvedPuzzle.Event.ID != eventId) {
                    continue;
                }

                // If the request is asking whether certain puzzles are solved, check if it's
                // in the set and, if so, put it in the list of solved puzzles to inform the
                // requester of.
                    
                if (queryPuzzleIdSet != null && queryPuzzleIdSet.Contains(solvedPuzzle.ID)) {
                    solvedPuzzles.Add(solvedPuzzle.ID);
                }

                // When counting solves, only count puzzles if they're not in the group that doesn't
                // count toward the solve count, and only if they're worth at least 10 points.

                if (groupExcludedFromSolveCount == null || solvedPuzzle.Group != groupExcludedFromSolveCount) {
                    if (solvedPuzzle.SolveValue >= 10) {
                        maxSolveCount += 1;
                    }
                }

                // If the user solved a cheat code, treat it as a solve count of 1000.

                if (solvedPuzzle.IsCheatCode) {
                    maxSolveCount += 1000;
                }
            }

            // If the requester is asking for puzzle pieces (by setting request.MinSolveCount != null)
            // and if there are pieces of the puzzle that the requester has now earned but hasn't
            // yet received, because the maximum solve count they've earned is at least as high
            // as the minimum solve count the requester is asking for, then return those pieces.
            // Note that request.MinSolveCount is the minimum solve count of tokens the requester
            // *hasn't* seen yet.

            if (request.MinSolveCount != null && maxSolveCount >= request.MinSolveCount) {
                List<Piece> pieces = await (from piece in context.Pieces
                                            where piece.ProgressLevel >= request.MinSolveCount && piece.ProgressLevel <= maxSolveCount
                                            select piece).ToListAsync();
                response.SetMinAndMaxSolveCountAndPieces(request.MinSolveCount.Value, maxSolveCount, pieces);
            }

            // If we found some solved puzzles in request.QueryPuzzleIds, return those in the
            // response.

            if (solvedPuzzles.Count > 0) {
                response.SetSolvedPuzzles(solvedPuzzles);
            }
        }

        /// <summary>
        ///   This routine stores in the database any annotations the requester has uploaded.
        /// </summary>
        private async Task StoreAnnotations(DecodedSyncRequest request, SyncResponse response, int puzzleId, int teamId)
        {
            if (request.AnnotationRequests == null) {
                return;
            }

            foreach (var annotationRequest in request.AnnotationRequests) {
                // Try to generate this as a new annotation, with version 1.

                Annotation annotation = new Annotation();
                annotation.PuzzleID = puzzleId;
                annotation.TeamID = teamId;
                annotation.Key = annotationRequest.key;
                annotation.Version = 1;
                annotation.Contents = annotationRequest.contents;
                annotation.Timestamp = DateTime.Now;

                try {
                    context.Annotations.Add(annotation);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException) {
                    // If the insert fails, there must already be an annotation there with the
                    // same puzzle ID, team ID, and key.  So we need to update the existing one.
                    // As we do so, we increment its version number and update its timestamp.
                    //
                    // You may wonder why we're using ExecuteSqlCommandAsync instead of "normal"
                    // Entity Framework database functions.  The answer is that we need to atomically
                    // update the Version field of the record, and Entity Framework has no way of
                    // expressing that directly.
                    //
                    // The reason we want to update the version number atomically is that we rely
                    // on the version number being a unique identifier of an annotation.  We don't
                    // want the following scenario:
                    //
                    // Alice tries to set the annotation for key 17 to A, and simultaneously Bob
                    // tries to set it to B.  Each reads the current version number, finds it to be
                    // 3, and updates the annotation to have version 4.  Both of these updates may
                    // succeed, but one will overwrite the other; let's say Bob's write happens last
                    // and "wins".  So Alice may believe that version 4 is A when actually version 4
                    // is B.  When Alice asks for the current version, she'll be told it's version 4,
                    // and Alice will believe this means it's A.  So Alice will believe that A is
                    // what's stored in the database even though it's not.  Alice and Bob's computers
                    // will display different annotations for the same key, indefinitely.
                    //
                    // Note that we need a version number because the timestamp isn't guaranteed to
                    // be unique.  So in the example above Alice and Bob might wind up updating with
                    // the same timestamp.
                    //
                    // You may also wonder why we use DateTime.Now instead of letting the database
                    // assign the timestamp itself.  The reason is that the database might be running
                    // on a different machine than the puzzle server, and it might be using a different
                    // time zone.

                    // First, detach the annotation from the context so the context doesn't think the annotation is in the database.
                    context.Entry(annotation).State = EntityState.Detached;
                    
                    try {
                        var sqlCommand = "UPDATE Annotations SET Version = Version + 1, Contents = @Contents, Timestamp = @Timestamp WHERE PuzzleID = @PuzzleID AND TeamID = @TeamID AND [Key] = @Key";
                        int result = await context.Database.ExecuteSqlCommandAsync(sqlCommand,
                                                                                   new SqlParameter("@Contents", annotationRequest.contents),
                                                                                   new SqlParameter("@Timestamp", DateTime.Now),
                                                                                   new SqlParameter("@PuzzleID", puzzleId),
                                                                                   new SqlParameter("@TeamID", teamId),
                                                                                   new SqlParameter("@Key", annotationRequest.key));
                        if (result != 1) {
                            response.AddError("Annotation update failed");
                        }
                    }
                    catch (DbUpdateException) {
                        response.AddError("Encountered error while trying to update annotation");
                    }
                    catch (Exception) {
                        response.AddError("Miscellaneous error while trying to update annotation");
                    }
                }
            }
        }

        /// <summary>
        ///   This routine fetches the list of annotations that the requester's team has made since the last
        ///   time the requester got a list of annotations.
        /// </summary>
        private async Task GetAnnotationsRequesterLacks(DecodedSyncRequest request, SyncResponse response, int puzzleId, int teamId)
        {
            // We get the current time (which we'll later set as the "sync time") *before* we do the
            // query, so that it's a conservative estimate of when the sync happened.

            DateTime now = DateTime.Now;

            List<Annotation> annotations;

            if (request.LastSyncTime == null) {
                // If the requester didn't specify a last-sync time, then provide all the annotations from their team
                // for this puzzle.
                annotations = await (from a in context.Annotations
                                     where a.PuzzleID == puzzleId && a.TeamID == teamId
                                     select a).ToListAsync();
            }
            else {
                // We know the time of the last request, so in theory we should just return only
                // annotations that are from that time or later.  But, it could happen that an
                // update that was concurrent with the previous sync request didn't make it into the
                // returned list but nevertheless got a timestamp after that sync request.  Also,
                // there may be multiple web servers, with slightly unsynchronized clocks.  So, for
                // safety, we subtract five seconds.  This may cause us to unnecessarily fetch and
                // return an annotation the requester already knows about, but this is harmless: The
                // requester will see that the annotation has the same version number as the one
                // the requester already knows about for that key, and ignore it.
                
                var lastSyncTimeMinusSlop = request.LastSyncTime.Value.AddSeconds(-5);
                annotations = await (from a in context.Annotations
                                     where a.PuzzleID == puzzleId && a.TeamID == teamId && a.Timestamp >= lastSyncTimeMinusSlop
                                     select a).ToListAsync();
            }

            response.SetSyncTimeAndAnnotations(now, annotations);
        }

        public async Task<dynamic> GetSyncResponse(int eventId, int teamId, int puzzleId, List<int> query_puzzle_ids, int? min_solve_count,
                                                   string annotations, string last_sync_time)
        {
            SyncResponse response = new SyncResponse();

            // Get information for the puzzle that's being sync'ed.

            Puzzle thisPuzzle = await context.Puzzles.Where(puz => puz.ID == puzzleId).FirstOrDefaultAsync();
            if (thisPuzzle == null) {
                response.AddError("Could not find the puzzle you tried to sync");
                return response.GetResult();
            }

            // Check to make sure that they're not trying to sync a puzzle and event that don't go together.
            // That could be a security issue, allowing them to unlock pieces for a puzzle using the progress
            // they made in a whole different event!
            
            if (thisPuzzle.Event.ID != eventId) {
                response.AddError("That puzzle doesn't belong to that event.");
                return response.GetResult();
            }

            // Get the site version for this puzzle, so that if it's changed since the last time the
            // requester sync'ed, the requester will know to reload itself.

            response.SetSiteVersion(GetSiteVersion(thisPuzzle));

            // Decode the request.  If there are any errors, return an error response.

            DecodedSyncRequest request = new DecodedSyncRequest(query_puzzle_ids, min_solve_count, annotations, last_sync_time,
                                                                thisPuzzle.MaxAnnotationKey, ref response);
            if (response.HasError) {
                return response.GetResult();
            }

            // Do any processing that requires fetching the list of all puzzles this team has
            // solved.  Pass thisPuzzle.Group as the groupExcludedFromSolveCount parameter so that,
            // when counting solves, we don't count solves in the same group as this puzzle as part
            // of the solve count.

            await HandleSyncAspectsRequiringListOfSolvedPuzzles(request, response, thisPuzzle.Group, teamId, eventId);

            // Store any annotations the requester provided

            await StoreAnnotations(request, response, thisPuzzle.ID, teamId);
            if (response.HasError) {
                return response.GetResult();
            }

            // Fetch and return any annotations that the requester may not have yet

            await GetAnnotationsRequesterLacks(request, response, thisPuzzle.ID, teamId);

            return response.GetResult();
        }
    }

    [Route("{eventId}/api/Sync/{puzzleId}")]
    public class SyncController : Controller
    {
        private readonly PuzzleServerContext context;
        private readonly UserManager<IdentityUser> userManager;

        public SyncController(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        
        // POST api/Sync
        [HttpPost]
        public async Task<IActionResult> Post(string eventId, int puzzleId, List<int> query_puzzle_ids, int? min_solve_count, string annotations,
                                              string last_sync_time)
        {
            // Find what team this user is on, relative to the event.

            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);
            if (currentEvent == null) { return Unauthorized(); }
            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);
            if (user == null) { return Unauthorized(); }
            Team team = await UserEventHelper.GetTeamForPlayer(context, currentEvent, user);
            if (team == null) { return Unauthorized(); }

            var helper = new SyncHelper(context);
            var response = await helper.GetSyncResponse(currentEvent.ID, team.ID, puzzleId, query_puzzle_ids, min_solve_count,
                                                        annotations, last_sync_time);
            return Json(response);
        }
    }
}
