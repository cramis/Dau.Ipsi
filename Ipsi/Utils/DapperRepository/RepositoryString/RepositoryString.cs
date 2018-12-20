using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using DapperRepositoryException;

namespace DapperRepository
{
    public interface IRepositoryString
    {
        string SelectString<T>(T model);

        string SelectString<T>(T model, params ParamColumn[] args);
        string InsertStr<T>(T model);
        string UpdateStr<T>(T model);
        string MergeStr<T>(T model);
        string DeleteStr<T>(T model);
        string DeleteStr<T>(T model, params ParamColumn[] args);


    }
    public class BaseRepositoryString : IRepositoryString
    {

        private readonly IORMHelper helper;

        private readonly string ParamMark = ":";
        private readonly string DBNowDatefunction = "SYSDATE";


        public BaseRepositoryString(IORMHelper helper)
        {
            this.helper = helper;
        }

        public BaseRepositoryString(IORMHelper helper, string ParamMark, string DBNowDatefunction)
        {
            this.helper = helper;
            this.ParamMark = ParamMark;
            this.DBNowDatefunction = DBNowDatefunction;

        }

        public string SelectString<T>(T model)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            str.AppendFormat("SELECT * FROM {0}", tableName);
            str.Append(" WHERE 1=1");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                if (p.GetValue(model) != null)
                {
                    str.AppendFormat(" AND {0}.{1} = {2}{3}", tableName, columnName, ParamMark, p.Name);
                    count++;
                }

            }

            return str.ToString().TrimEnd();
        }

        public string SelectString<T>(T model, params ParamColumn[] args)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            str.AppendFormat("SELECT * FROM {0}", tableName);
            str.Append(" WHERE 1=1");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var a in args)
            {
                if (string.IsNullOrWhiteSpace(a.Operator))
                {
                    throw new DBOperatorNotFoundException("연산자를 존재하지 않습니다. 연산자가 반드시 입력되어야 합니다. 예) '>', '<=', 'between' 등 ");
                }

                StringBuilder operatorAndvalues = new StringBuilder();

                switch (a.Operator.ToUpper())
                {
                    case "BETWEEN":
                        operatorAndvalues.AppendFormat("BETWEEN {0} AND {1}", a.Operator_values[0], a.Operator_values[1]);
                        break;
                    case "IS NULL":
                        operatorAndvalues.Append("IS NULL");
                        break;
                    case "IS NOT NULL":
                        operatorAndvalues.Append("IS NOT NULL");
                        break;
                    default:
                        operatorAndvalues.AppendFormat("{0} {1}", a.Operator.ToUpper(), a.Operator_values[0]);
                        break;
                }

                str.AppendFormat(" AND {0}.{1} {2}", tableName, a.COLUMN_NAME, operatorAndvalues);
                count++;

            }

            return str.ToString().TrimEnd();
        }
        public string InsertStr<T>(T model)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            var props = model.GetType().GetProperties();

            long count = 0;
            long pk_Count = 0;
            str.AppendFormat("INSERT INTO {0} ( ", tableName);

            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (helper.CheckKey(p))
                        {
                            pk_Count++;
                        }
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }
                        str.Append(columnName);
                        count++;
                    }
                    // AutoCreate이면서 PK인 경우는 pk_Count 자동 증가해줌
                    if (p.GetValue(model) == null && helper.CheckKey(p) && helper.CheckAutoCreate(p))
                    {
                        pk_Count++;
                    }

                    // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                    if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                    {
                        throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                    }
                }
            }

            if (pk_Count < 1)
            {
                throw new PkNotFoundException("Insert를 할때에는 Primary Key가 한개 이상 존재해야 합니다.");
            }

            str.Append(" ) VALUES ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckCreatedDate(p))
                        {
                            str.Append(DBNowDatefunction);
                        }
                        else
                        {
                            str.Append(ParamMark + p.Name);
                        }
                        count++;
                    }

                    // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                    if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                    {
                        throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                    }
                }
            }
            str.Append(" )");

            return str.ToString().TrimEnd();
        }
        public string UpdateStr<T>(T model, bool IsPosibleNullValue)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            var props = model.GetType().GetProperties();

            long count = 0;

            str.AppendFormat("UPDATE {0} SET ", tableName);
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                {
                    throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                }


                if (IsPosibleNullValue || p.GetValue(model) != null || helper.CheckLastModifiedDate(p))
                {
                    if (helper.CheckCreatedDate(p) || helper.CheckKey(p))
                    {
                        continue;
                    }

                    if (!helper.CheckIgnore(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckLastModifiedDate(p))
                        {
                            str.AppendFormat("{0} = {1}", columnName, DBNowDatefunction);
                        }
                        else if (p.GetValue(model) == null)
                        {
                            str.AppendFormat("{0} = NULL", columnName);
                        }
                        else if (helper.CheckCreatedDate(p) == false)
                        {
                            str.AppendFormat("{0} = {1}{2}", columnName, ParamMark, p.Name);
                        }
                        count++;
                    }
                }


            }
            str.Append(" WHERE ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                if (helper.CheckKey(p))
                {
                    if (p.GetValue(model) == null)
                    {
                        throw new PkNotFoundException("Update를 할때에는 Primary Key에 반드시 값이 존재해야 합니다.");
                    }

                    if (count >= 1)
                    {
                        str.Append(" AND ");
                    }
                    str.AppendFormat("{0} = {1}{2}", columnName, ParamMark, p.Name);
                    count++;
                }

            }


            return str.ToString().TrimEnd();
        }

        public string UpdateStr<T>(T model)
        {
            return UpdateStr(model, false);
        }

        public virtual string MergeStr<T>(T model, bool IsPosibleNullValue)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            var props = model.GetType().GetProperties();

            long count = 0;

            str.AppendFormat("MERGE INTO {0} USING DUAL ON ( ", tableName);

            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                if (helper.CheckKey(p) && p.GetValue(model) != null)
                {
                    if (count >= 1)
                    {
                        str.Append(" AND ");
                    }
                    str.AppendFormat("{0} = {1}{2}", columnName, ParamMark, p.Name);
                    count++;
                }

            }

            if (count < 1)
            {
                throw new PkNotFoundException("Update를 할때에는 Primary Key에 반드시 값이 존재해야 합니다.");
            }


            str.Append(" ) WHEN MATCHED THEN UPDATE SET ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                if (IsPosibleNullValue || p.GetValue(model) != null || helper.CheckLastModifiedDate(p))
                {
                    if (helper.CheckCreatedDate(p) || helper.CheckKey(p))
                    {
                        if (!helper.CheckLastModifiedDate(p))
                        {
                            continue;
                        }
                    }

                    if (!helper.CheckIgnore(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckLastModifiedDate(p))
                        {
                            str.AppendFormat("{0} = {1}", columnName, DBNowDatefunction);
                        }
                        else if (p.GetValue(model) == null)
                        {
                            str.AppendFormat("{0} = NULL", columnName);
                        }
                        else if (helper.CheckCreatedDate(p) == false)
                        {
                            str.AppendFormat("{0} = {1}{2}", columnName, ParamMark, p.Name);
                        }
                        count++;
                    }
                }

                // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                {
                    throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                }
            }

            str.Append(" WHEN NOT MATCHED THEN INSERT ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }
                        str.Append(columnName);
                        count++;
                    }

                    // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                    if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                    {
                        throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                    }
                }

            }

            str.AppendFormat(" ) VALUES ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckCreatedDate(p))
                        {
                            str.Append(DBNowDatefunction);
                        }
                        else
                        {
                            str.Append(ParamMark + p.Name);
                        }
                        count++;
                    }

                    // 필수 입력인데 값이 안들어가 있는 경우 RequiredValueNotFoundException
                    if (p.GetValue(model) == null && helper.CheckRequiredColumn(p))
                    {
                        throw new RequiredValueNotFoundException(p.GetValue(model) + "에 반드시 값이 들어가야 할 컬럼입니다.");
                    }
                }
            }
            str.Append(" )");

            return str.ToString().TrimEnd();

        }

        public string MergeStr<T>(T model)
        {
            return MergeStr(model, false);
        }
        public string DeleteStr<T>(T model)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            str.AppendFormat("DELETE FROM {0}", tableName);
            str.Append(" WHERE 1=1");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);

                if (p.GetValue(model) != null)
                {
                    str.AppendFormat(" AND {0} = {1}{2}", columnName, ParamMark, p.Name);
                    count++;
                }

            }

            return str.ToString().TrimEnd();
        }

        public string DeleteStr<T>(T model, params ParamColumn[] args)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            str.AppendFormat("DELETE FROM {0}", tableName);
            str.Append(" WHERE 1=1");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var a in args)
            {
                if (string.IsNullOrWhiteSpace(a.Operator))
                {
                    throw new DBOperatorNotFoundException("연산자를 존재하지 않습니다. 연산자가 반드시 입력되어야 합니다. 예) '>', '<=', 'between' 등 ");
                }

                StringBuilder operatorAndvalues = new StringBuilder();

                switch (a.Operator.ToUpper())
                {
                    case "BETWEEN":
                        operatorAndvalues.AppendFormat("BETWEEN {0} AND {1}", a.Operator_values[0], a.Operator_values[1]);
                        break;
                    case "IS NULL":
                        operatorAndvalues.Append("IS NULL");
                        break;
                    case "IS NOT NULL":
                        operatorAndvalues.Append("IS NOT NULL");
                        break;
                    default:
                        operatorAndvalues.AppendFormat("{0} {1}", a.Operator.ToUpper(), a.Operator_values[0]);
                        break;
                }

                str.AppendFormat(" AND {0} {1}", a.COLUMN_NAME, operatorAndvalues);
                count++;

            }

            return str.ToString().TrimEnd();
        }
    }



    public class OracleRepositoryString : BaseRepositoryString
    {
        public OracleRepositoryString(IORMHelper helper) : base(helper, ":", "SYSDATE")
        {
        }
    }

    public class MysqlRepositoryString : BaseRepositoryString
    {
        private readonly IORMHelper helper;
        public MysqlRepositoryString(IORMHelper helper) : base(helper, "@", "NOW()")
        {
            this.helper = helper;
        }

        public override string MergeStr<T>(T model, bool IsPosibleNullValue)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            var props = model.GetType().GetProperties();

            long count = 0;
            long pk_Count = 0;
            str.AppendFormat("INSERT INTO {0} ( ", tableName);

            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (helper.CheckKey(p))
                        {
                            pk_Count++;
                        }
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }
                        str.Append(columnName);
                        count++;
                    }
                }
            }

            if (pk_Count < 1)
            {
                throw new PkNotFoundException("Insert를 할때에는 Primary Key가 한개 이상 존재해야 합니다.");
            }

            str.Append(" ) VALUES ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckCreatedDate(p))
                        {
                            str.Append("NOW()");
                        }
                        else
                        {
                            str.Append("@" + p.Name);
                        }
                        count++;
                    }
                }
            }
            str.Append(" ) ON DUPLICATE KEY UPDATE ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = helper.ColumnName(p);


                if (IsPosibleNullValue || p.GetValue(model) != null || helper.CheckLastModifiedDate(p))
                {
                    if (helper.CheckCreatedDate(p) || helper.CheckKey(p))
                    {
                        continue;
                    }

                    if (!helper.CheckIgnore(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckLastModifiedDate(p))
                        {
                            str.AppendFormat("{0} = {1}", columnName, "NOW()");
                        }
                        else if (p.GetValue(model) == null)
                        {
                            str.AppendFormat("{0} = NULL", columnName);
                        }
                        else if (helper.CheckCreatedDate(p) == false)
                        {
                            str.AppendFormat("{0} = {1}{2}", columnName, "@", p.Name);
                        }
                        count++;
                    }
                }
            }

            return str.ToString().TrimEnd();

        }
    }


    public class SqliteRepositoryString : BaseRepositoryString
    {
        private readonly IORMHelper helper;
        public SqliteRepositoryString(IORMHelper helper) : base(helper, ":", "DATETIME('NOW','LOCALTIME')")
        {
            this.helper = helper;
        }

        public override string MergeStr<T>(T model, bool IsPosibleNullValue)
        {
            StringBuilder str = new StringBuilder("");
            string tableName = helper.GetTableName(model.GetType());
            var props = model.GetType().GetProperties();

            long count = 0;
            long pk_Count = 0;

            str.AppendFormat("INSERT OR REPLACE INTO {0} (", tableName);

            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (helper.CheckKey(p))
                        {
                            pk_Count++;
                        }
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }
                        str.Append(columnName);
                        count++;
                    }
                }
            }

            if (pk_Count < 1)
            {
                throw new PkNotFoundException("Merge 할때에는 Primary Key가 한개 이상 존재해야 합니다.");
            }

            str.Append(" ) VALUES ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!helper.CheckIgnore(p))
                {
                    var columnName = helper.ColumnName(p);

                    if (p.GetValue(model) != null || helper.CheckCreatedDate(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(", ");
                        }

                        if (helper.CheckCreatedDate(p))
                        {
                            str.Append("DATETIME('NOW','LOCALTIME')");
                        }
                        else
                        {
                            str.Append(":" + p.Name);
                        }
                        count++;
                    }
                }
            }
            str.Append(" )");

            return str.ToString().TrimEnd();

        }
    }
}