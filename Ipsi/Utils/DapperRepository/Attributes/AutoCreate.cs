
namespace DapperRepository
{

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class AutoCreate : System.Attribute
    {
        private string name;

        public AutoCreate()
        {
            name = "AutoCreate";
        }

        public string getName()
        {
            return name;
        }
    }
}