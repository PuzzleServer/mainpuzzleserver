using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace ServerCore.DataModel
{
    [PersonalData]
    public class PuzzleUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Links the puzzle user to their authentication identity
        /// </summary>
        [ForeignKey("AspNetUsers.Id")]
        public string IdentityUserId { get; set; }

        public string Name { get; set; }
        public string EmployeeAlias { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string TShirtSize { get; set; }
        public bool VisibleToOthers { get; set; }

        public static PuzzleUser GetPuzzleUser(string identityUserId, PuzzleServerContext dbContext)
        {
            return dbContext.PuzzleUsers.Where(user => user.IdentityUserId == identityUserId).FirstOrDefault();
        }
    }
}
