namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IgnoreColumn : System.Attribute
    {
        private string name;

        public IgnoreColumn()
        {
            name = "Ignore";
        }

        public string getName()
        {
            return name;
        }
    }
}