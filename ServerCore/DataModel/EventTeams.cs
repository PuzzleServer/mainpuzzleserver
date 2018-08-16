namespace ServerCore.DataModel
{
    public class EventTeams
    {
        // ID for row
        public int ID { get; set; }

        // Foreign Key event table
        public int EventID { get; set; }

        // Foreign Key teams table
        public int TeamID { get; set; }
    }
}