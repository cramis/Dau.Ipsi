namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class KeyColumn : System.Attribute
    {
        private string name;

        public KeyColumn()
        {
            name = "pk";
        }

        public string getName()
        {
            return name;
        }
    }
}