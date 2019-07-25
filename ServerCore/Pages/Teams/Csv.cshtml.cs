using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class CsvModel : TeamListBase
    {
        public CsvModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) 
            : base(serverContext, userManager)
        {

        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTeamDataAsync();
            var teamCsvRecords = (from team in Teams
                                  select new
                                  {
                                      Name = team.Name,
                                      Size = PlayerCountByTeamID[team.ID],
                                      Room = team.CustomRoom,
                                      Email = team.PrimaryContactEmail,
                                      PrimaryPhoneNumber = team.PrimaryPhoneNumber,
                                      SecondaryPhoneNumber = team.SecondaryPhoneNumber,

                                  }).ToList();

            return CsvCreationHelper.CreateCsvFileResult(teamCsvRecords, "Teams.csv");
        }
    }
}