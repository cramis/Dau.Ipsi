namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class LastModifiedDate : System.Attribute
    {
        private string name;

        public LastModifiedDate()
        {
            name = "modified";
        }

        public string getName()
        {
            return name;
        }
    }
}