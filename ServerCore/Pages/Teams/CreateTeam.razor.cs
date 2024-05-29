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
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult("Team names cannot be left blank.");
            }

            if (name.Length > 50) 
            {
                return new ValidationResult("Team names must be fewer than 50 characters.");    
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
        [RegularExpression("\\S+.*")]
        [TeamUniquenessValidation]
        public string Name { get; set; }

        public string CustomRoom { get; set; }

        [Required]
        [EmailListValidation]
        public string PrimaryContactEmail { get; set; }

        [Phone]
        public string PrimaryPhoneNumber { get; set; }
        [Phone]
        public string SecondaryPhoneNumber { get; set; }

        public bool IsLookingForTeammates { get; set; }
        public string Bio { get; set; }
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

        protected override async Task OnParametersSetAsync()
        {
            TeamModel.EventId = Event.ID;

            if (EventRole == EventRole.play && await _context.Teams.Where((t) => t.EventID == Event.ID).CountAsync() >= Event.MaxNumberOfTeams)
            {
                EventFull = true;
            }

            await base.OnParametersSetAsync();
        }

        public async Task OnSubmit()
        {
            Team team = new Team
            {
                EventID = Event.ID,
                Name = TeamModel.Name,
                CustomRoom = TeamModel.CustomRoom,
                PrimaryContactEmail = TeamModel.PrimaryContactEmail,
                PrimaryPhoneNumber = TeamModel.PrimaryPhoneNumber,
                SecondaryPhoneNumber = TeamModel.SecondaryPhoneNumber,
                IsLookingForTeammates = TeamModel.IsLookingForTeammates,
                Bio = TeamModel.Bio,                
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
