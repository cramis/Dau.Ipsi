namespace DapperRepository
{
    public class ColumnInfo
    {
        public ColumnInfo()
        {

        }

        public ColumnInfo(string COLUMN_NAME)
        {
            this.COLUMN_NAME = COLUMN_NAME;
        }


        public string COLUMN_NAME { get; set; }

        public string COLUMN_VALUE { get; set; }
    }
}