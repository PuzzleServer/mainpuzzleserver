namespace ServerCore.DataModel
{
    public class EventAdmins
    {
        // ID for row
        public int ID { get; set; }

        // Foreign key event table
        public int EventID { get; set; }

        // Foreign key user table
        public int UserID { get; set; }

    }
}