using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerCore.DataModel;

namespace ServerCore.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CanCreateEvent()
        {
            DbContextOptions<PuzzleServerContext> options = new DbContextOptionsBuilder<PuzzleServerContext>().UseInMemoryDatabase("TestChanges").Options;
            using (PuzzleServerContext context = new PuzzleServerContext(options))
            {
                Event Event = new Event();
                Event.Name = "Check";
                context.Events.Add(Event);
                Assert.AreEqual(1, Event.ID);
            }
        }
    }
}
