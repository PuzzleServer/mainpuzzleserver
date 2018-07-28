namespace ServerCore.DataModel
{
    public class EventTeams
    {
        // Foreign Key event table
        public int EventID { get; set; }

        // Foreign Key teams table
        public int TeamID { get; set; }
    }
}