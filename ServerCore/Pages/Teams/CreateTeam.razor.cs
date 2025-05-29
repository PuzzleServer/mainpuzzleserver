using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;
using System.Linq;

namespace ServerCore.Pages.Teams
{
    class EmailListValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string emailList = value as string;
            if (string.IsNullOrWhiteSpace(emailList))
            {
                return new ValidationResult("An email is required.");
            }

            if (!MailHelper.IsValidEmail(emailList))
            {
                return new ValidationResult("Invalid primary contact email.");
            }

            return ValidationResult.Success;
        }
    }

    class TeamUniquenessValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PuzzleServerContext puzzleServerContext = validationContext.GetRequiredService<PuzzleServerContext>();
            TeamModel teamModel = (TeamModel)validationContext.ObjectInstance;
            string name = value as string;
            name = TeamHelper.UnicodeSanitizeTeamName(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult("Team names cannot be left blank.");
            }

            if (TeamHelper.IsTeamNameTaken(puzzleServerContext, teamModel.EventId, name))
            {
                return new ValidationResult("Another team has this name.");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Team signup class based off of <see cref="Team"/>
    /// </summary>
    public class TeamModel
    {
        public int EventId { get; set; }

        [Required]
        [TeamUniquenessValidation]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string CustomRoom { get; set; }

        [Required]
        [EmailListValidation]
        [MaxLength(50)]
        public string PrimaryContactEmail { get; set; }

        [Phone]
        [MaxLength(20)]
        public string PrimaryPhoneNumber { get; set; }
        [Phone]
        [MaxLength(20)]
        public string SecondaryPhoneNumber { get; set; }

        public bool IsLookingForTeammates { get; set; }
        [MaxLength(500)]
        public string Bio { get; set; }

        [Required]
        public bool? IsRemoteTeam { get; set; }
    }

    public partial class CreateTeam
    {
        [Parameter]
        public Event Event { get; set; }

        [Parameter]
        public EventRole EventRole { get; set; }

        [Parameter]
        public int LoggedInUserId { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        public TeamModel TeamModel { get; set; } = new TeamModel();

        public bool EventFull { get; set; }
        public bool LocalTeamsFull { get; set; }
        public bool RemoteTeamsFull { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            TeamModel.EventId = Event.ID;

            if (EventRole == EventRole.play && await _context.Teams.Where((t) => t.EventID == Event.ID).CountAsync() >= Event.MaxNumberOfTeams)
            {
                EventFull = true;
            }

            if (EventRole == EventRole.play && await _context.Teams.Where((t) => t.EventID == Event.ID && !t.IsRemoteTeam).CountAsync() >= Event.MaxNumberOfLocalTeams)
            {
                LocalTeamsFull = true;
            }

            if (EventRole == EventRole.play && await _context.Teams.Where((t) => t.EventID == Event.ID && t.IsRemoteTeam).CountAsync() >= Event.MaxNumberOfRemoteTeams)
            {
                RemoteTeamsFull = true;
            }

            await base.OnParametersSetAsync();
        }

        public async Task OnSubmit()
        {
            Team team = new Team
            {
                EventID = Event.ID,
                Name = TeamHelper.UnicodeSanitizeTeamName(TeamModel.Name),
                CustomRoom = TeamModel.CustomRoom,
                PrimaryContactEmail = TeamModel.PrimaryContactEmail,
                PrimaryPhoneNumber = TeamModel.PrimaryPhoneNumber,
                SecondaryPhoneNumber = TeamModel.SecondaryPhoneNumber,
                IsLookingForTeammates = TeamModel.IsLookingForTeammates,
                Bio = TeamModel.Bio, 
                IsRemoteTeam = TeamModel.IsRemoteTeam.GetValueOrDefault()
            };

            int? idToAdd = null;
            if (EventRole == EventRole.play)
            {
                idToAdd = LoggedInUserId;
            }

            await TeamHelper.CreateTeamAsync(_context, team, Event, idToAdd);
            Team newTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUserId);
            NavigationManager.NavigateTo($"/{Event.EventID}/{EventRole}/Teams/{newTeam.ID}/Details", true);
        }
    }
}
