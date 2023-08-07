namespace ServerCore.Pages.Teams
{
    public enum SignupStrategy
    {
        Create,
        Join,
        Auto
    }

    public partial class SignupHub
    {
        public string TestString { get; set; } = "OrigValue";

        private string strategy;

        public string Strategy
        {
            get
            {
                return strategy;
            }
            set
            {
                strategy = value;
            }
        }
    }
}
