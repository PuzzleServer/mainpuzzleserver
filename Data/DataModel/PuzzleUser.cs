using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Custom user object that holds the puzzle user specific data (i.e. the data that's used by the puzzle system, not the data used for auth)
    /// </summary>
    [PersonalData]
    public class PuzzleUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Links the puzzle user to their authentication identity
        /// </summary>
        [ForeignKey("AspNetUsers.Id")]
        [Required]
        public string IdentityUserId { get; set; }

        public string Name { get; set; }
        public string EmployeeAlias { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TShirtSize { get; set; }
        public bool VisibleToOthers { get; set; }

        public static PuzzleUser GetPuzzleUser(string identityUserId, PuzzleServerContext dbContext)
        {
            return dbContext.PuzzleUsers.Where(user => user.IdentityUserId == identityUserId).FirstOrDefault();
        }
    }
}
