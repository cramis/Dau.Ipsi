namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class Table : System.Attribute
    {
        private string name;

        public Table(string name)
        {
            this.name = name;
        }


        public string GetName()
        {
            return this.name;
        }

    }
}