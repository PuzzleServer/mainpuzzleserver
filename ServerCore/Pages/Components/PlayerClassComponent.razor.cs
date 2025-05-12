using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Migrations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Components
{
    public partial class PlayerClassComponent
    {
        [Parameter]
        public int EventID { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        public string ClassName { get; set; }
        public int ClassOrder { get; set; }

        List<PlayerClass> PlayerClasses = new List<PlayerClass>();

        protected override async Task OnParametersSetAsync()
        {
            await UpdatePlayerClasses();
            await base.OnParametersSetAsync();
        }

        private async Task OnAddClick()
        {
            await OnPlayerClassAddedAsync(ClassName, ClassOrder);
            await UpdatePlayerClasses();
        }

        private async Task OnDeleteClick(int classId)
        {
            await OnPlayerClassDeletedAsync(classId);
            await UpdatePlayerClasses();
        }

        async Task OnPlayerClassAddedAsync(string playerClassName, int playerClassOrder)
        {
            PlayerClass newClass = new PlayerClass() { EventID = EventID, Name = playerClassName, Order = playerClassOrder };
            _context.PlayerClasses.Add(newClass);
            await _context.SaveChangesAsync();
        }

        async Task OnPlayerClassDeletedAsync(int classId)
        {
            PlayerClass deleteClass = await _context.PlayerClasses.Where(c => c.ID == classId).FirstOrDefaultAsync();
            _context.PlayerClasses.Remove(deleteClass);
            await _context.SaveChangesAsync();
        }

        private async Task UpdatePlayerClasses()
        {
            PlayerClasses = await (from classes in _context.PlayerClasses
                                   where classes.EventID == EventID
                                   orderby classes.Order
                                   select classes).ToListAsync();
        }
    }
}
