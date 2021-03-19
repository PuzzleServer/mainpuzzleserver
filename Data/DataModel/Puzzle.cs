using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace ServerCore.DataModel
{
    public enum PieceMetaUsage
    {
        None,
        GroupOnly,
        EntireEvent
    }

    /// <summary>
    /// A Puzzle is the record of a solvable puzzle in the database
    /// Sometimes a Puzzle is used as a workaround for things like time prerequisites
    /// </summary>    
    public class Puzzle
    {
        public Puzzle()
        {
        }

        public Puzzle (Puzzle source)
        {
            // do not fill out the ID
            Event = source.Event;
            Name = source.Name;
            IsPuzzle = source.IsPuzzle;
            IsMetaPuzzle = source.IsMetaPuzzle;
            IsFinalPuzzle = source.IsFinalPuzzle;
            IsCheatCode = source.IsCheatCode;
            SolveValue = source.SolveValue;
            HintCoinsForSolve = source.HintCoinsForSolve;
            HintsAreCumulative = source.HintsAreCumulative;
            HasDataConfirmation = source.HasDataConfirmation;
            Token = source.Token;
            Group = source.Group;
            OrderInGroup = source.OrderInGroup;
            IsGloballyVisiblePrerequisite = source.IsGloballyVisiblePrerequisite;
            MinPrerequisiteCount = source.MinPrerequisiteCount;
            MinutesToAutomaticallySolve = source.MinutesToAutomaticallySolve;
            MinutesOfEventLockout = source.MinutesOfEventLockout;
            MaxAnnotationKey = source.MaxAnnotationKey;
            SupportEmailAlias = source.SupportEmailAlias;
            CustomURL = source.CustomURL;
            Description = source.Description;
            Errata = source.Errata;
            PieceMetaUsage = source.PieceMetaUsage;
        }

        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The event the puzzle is a part of
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// The event the puzzle is a part of
        /// </summary>
        [Required]
        public virtual Event Event { get; set; }

        /// <summary>
        /// The name of the puzzle
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// True only if not a "fake" puzzle like "READ THIS INSTRUCTION PAGE" or "START THE EVENT"
        /// </summary>
        public bool IsPuzzle { get; set; } = false;

        /// <summary>
        /// True if this is a meta puzzle
        /// </summary>
        public bool IsMetaPuzzle { get; set; } = false;

        /// <summary>
        /// True if this is the final puzzle that would lock a team's rank in the standings
        /// </summary>
        public bool IsFinalPuzzle { get; set; } = false;

        /// <summary>
        /// True if this puzzle is a "cheat code" (nee "Fast Forward") that should impact standings
        /// </summary>
        public bool IsCheatCode { get; set; }

        /// <summary>
        /// The solve value
        /// </summary>
        public int SolveValue { get; set; } = 0;

        /// <summary>
        /// The number of hint coins to award if the puzzle is solved
        /// </summary>
        public int HintCoinsForSolve { get; set; } = 0;

        /// <summary>
        /// whether to decrement future hint costs for this puzzle as hints are taken
        /// </summary>
        public bool HintsAreCumulative { get; set; }

        /// <summary>
        /// True if this puzzle has data confirmation
        /// </summary>
        public bool HasDataConfirmation { get; set; } = false;

        /// <summary>
        /// Reward if solved: Sometimes displayed publicly, sometimes used internally by meta engine
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Grouping key.
        /// Likely Puzzlehunt usage: name of the puzzle's module
        /// Likely Puzzleday usage: "Pregame" or blank
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The order in the group (commonly used for the intended release order of the puzzle)
        /// </summary>
        public int OrderInGroup { get; set; } = 0;

        /// <summary>
        /// If true, all authors can see this puzzle when picking prerequisites
        /// </summary>
        public bool IsGloballyVisiblePrerequisite { get; set; } = false;

        /// <summary>
        /// Minimum number of <see cref="Prerequisites.cs"/> that must be satisfied
        /// TODO: When the system is mature, set the default to 1 so new puzzles are not accidentally displayed.
        /// </summary>
        public int MinPrerequisiteCount { get; set; } = 0;

        /// <summary>
        /// Minutes from the time a puzzle is unlocked until it is automatically marked as solved.
        /// Note that the actual solve time may be different, as the computation of unlocks is somewhat throttled.
        /// </summary>
        public int? MinutesToAutomaticallySolve { get; set; } = null;

        /// <summary>
        /// How long to lock solvers out of the rest of the event
        /// </summary>
        public int MinutesOfEventLockout { get; set; }

        /// <summary>
        /// Some puzzles let teams store annotations describing their ongoing work, so they can share those
        /// with their teammates.  However, we don't want to let teams store arbitrary annotation data,
        /// since this could overwhelm our storage.  So this field says the maximum annotation key they
        /// may use.  So, for instance, if MaxAnnotationKey is 400, teams may only use annotation keys 1-400.
        /// In the common case, this value is 0, meaning teams aren't allowed to store annotations for
        /// this puzzle.
        /// </summary>
        public int MaxAnnotationKey { get; set; } = 0;

        /// <summary>
        /// This is an author-settable version number, so that the author can indicate that something
        /// about their puzzle has changed.  It also automatically gets updated by the files controller
        /// whenever the author uploads any new file for the puzle.  The sync API can be used by any
        /// client to find out the version number of a puzzle, which it can use for various purposes.
        /// For instance, an HTML page might unlock itself, or display a message to the user that the
        /// puzzle has been updated.
        /// </summary>
        public int PuzzleVersion { get; set; } = 0;

        /// <summary>
        /// All of the content files associated with this puzzle
        /// </summary>
        public virtual ICollection<ContentFile> Contents { get; set; }

        /// <summary>
        /// The email alias that players should use if they require support on the puzzle.
        /// If null, the event email address should be used instead.
        /// </summary>
        public string SupportEmailAlias { get; set; }

        /// <summary>
        /// Errata
        /// </summary>
        public string Errata { get; set; }

        /// <summary>
        /// A custom URL the puzzle should link to instead of an associated file
        /// </summary>
        [DataType(DataType.Url)]
        public string CustomURL { get; set; }

        /// <summary>
        /// Short description of the puzzle for authors or special pages
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines whether the puzzle can use the basic piece-driven meta.
        /// </summary>
        public PieceMetaUsage PieceMetaUsage { get; set; }

        /// <summary>
        /// Allows the puzzle to accept arbitrary answers without treating them as incorrect
        /// </summary>
        public bool IsFreeform { get; set; }

        //
        // WARNING: If you add new properties add them to the constructor as well so importing will work.
        //

        /// <summary>
        /// File for the main puzzle (typically a PDF containing the puzzle)
        /// </summary>
        [NotMapped]
        public ContentFile PuzzleFile
        {
            get
            {
                var PuzzleFiles = from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                                 where contentFile.FileType == ContentFileType.Puzzle
                                 select contentFile;
                Debug.Assert(PuzzleFiles.Count() <= 1);
                return PuzzleFiles.FirstOrDefault();
            }
        }

        /// <summary>
        /// File for the main answer (typically a PDF containing the answer)
        /// </summary>
        [NotMapped]
        public ContentFile AnswerFile
        {
            get
            {
                var answerPDFs = from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                                 where contentFile.FileType == ContentFileType.Answer
                                 select contentFile;
                Debug.Assert(answerPDFs.Count() <= 1);
                return answerPDFs.FirstOrDefault();
            }
        }


        /// <summary>
        /// Files associated with the puzzle, such as media
        /// </summary>
        [NotMapped]
        public IEnumerable<ContentFile> Materials
        {
            get
            {
                return from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                       where contentFile.FileType == ContentFileType.PuzzleMaterial
                       select contentFile;
            }
        }

        /// <summary>
        /// Files unlocked by solving this puzzle, often for metapuzzles
        /// </summary>
        [NotMapped]
        public IEnumerable<ContentFile> SolveTokenFiles
        {
            get
            {
                return from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                       where contentFile.FileType == ContentFileType.SolveToken
                       select contentFile;
            }
        }        

        public virtual List<Submission> Submissions { get; set; }

        //
        // WARNING: If you add new properties add them to the constructor as well so importing will work.
        //
    }
}
