namespace ServerCore.DataModel
{
    public class EventAdmins
    {
        // Foreign key event table
        public int EventID { get; set; }

        // Foreign key user table
        public int UserID { get; set; }

    }
}