namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class CreatedDate : System.Attribute
    {
        private string name;

        public CreatedDate()
        {
            name = "created";
        }

        public string getName()
        {
            return name;
        }
    }
}