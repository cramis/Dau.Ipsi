

namespace DapperRepository
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class BindingColumn : System.Attribute
    {
        private string name;

        public BindingColumn(string column)
        {
            this.name = column;
        }

        public string getName()
        {
            return name;
        }
    }
}