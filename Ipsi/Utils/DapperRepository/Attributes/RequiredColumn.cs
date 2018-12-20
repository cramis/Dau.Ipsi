namespace DapperRepository
{

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class RequiredColumn : System.Attribute
    {
        private string name;

        public RequiredColumn()
        {
            name = "Required";
        }

        public string getName()
        {
            return name;
        }
    }
}