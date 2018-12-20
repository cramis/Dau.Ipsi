using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using Dapper;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Serilog;

namespace Dau.Util
{
    public class DapperORM
    {
        private string TableName;
        public List<ColumnInfo> ColumnList { get; set; }
        private IDbConnection connection;
        private object Model;
        private readonly Serilog.ILogger logger;


        public DapperORM(IDbConnection conn, object model)
        {
            this.connection = conn;

            var t = model.GetType();
            string tableName = this.GetTableName(t).ToUpper();
            string synonym = this.GetTableSynonym(t).ToUpper();

            this.GetColumnList(tableName);


            if (synonym == "")
            {
                this.TableName = tableName;
            }
            else
            {
                this.TableName = synonym + "." + tableName;
            }


            this.Model = model;

            this.logger = Log.Logger; ;

        }


        public DapperORM(IDbConnection conn, string tableName) : this(conn, tableName, "") { }


        public DapperORM(IDbConnection conn, string tableName, string synonym)
        {
            this.connection = conn;

            this.GetColumnList(tableName);

            if (synonym == "")
            {
                this.TableName = tableName;
            }
            else
            {
                this.TableName = synonym + "." + tableName;
            }

            this.logger = Log.Logger;

        }

        public void SetTableName(string tableName)
        {
            this.TableName = tableName.ToUpper();

            this.GetColumnList(TableName);
        }

        public string GetTableName()
        {
            return this.TableName;
        }

        private StringBuilder GetListStr<T>(T model)
        {
            StringBuilder str = new StringBuilder("");
            str.Append("SELECT * FROM ");
            str.Append(this.TableName);
            str.Append(" WHERE ");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (p.GetValue(model) != null)
                    {

                        if (count >= 1)
                        {
                            str.Append(" AND ");
                        }
                        str.Append(columnName + " = :" + p.Name);
                        count++;
                    }
                }

            }

            if (count == 0)
            {
                throw new Exception("파라미터로 전달된 모델 객체에 값이 존재하는 프로퍼티가 하나 이상 존재해야 합니다.");
            }

            this.WriteLog("GetListStr query", str.ToString(), JsonConvert.SerializeObject(model));

            return str;
        }

        public List<T> GetList<T>(T model)
        {


            var list = connection.Query<T>(GetListStr(model).ToString(), model).ToList();

            return list;
        }



        public List<T> GetList<T>()
        {
            StringBuilder str = new StringBuilder("");
            str.Append("SELECT * FROM ");
            str.Append(this.TableName);



            var list = connection.Query<T>(str.ToString()).ToList();

            return list;
        }


        public async Task<List<T>> GetListAsync<T>(T model)
        {
            var list = await connection.QueryAsync<T>(GetListStr(model).ToString(), model);

            return list.ToList();
        }



        public async Task<List<T>> GetListAsync<T>()
        {
            StringBuilder str = new StringBuilder("");
            str.Append("SELECT * FROM ");
            str.Append(this.TableName);



            var list = await connection.QueryAsync<T>(str.ToString());

            return list.ToList();
        }

        public int ExcuteScalar<T>(string sql, T model)
        {
            var result = connection.ExecuteScalar<int>(sql, model);

            return result;
        }

        public async Task<int> ExcuteScalarAsync<T>(string sql, T model)
        {
            var result = await connection.ExecuteScalarAsync<int>(sql, model);

            return result;
        }

        private StringBuilder GetItemStr<T>(T model)
        {
            StringBuilder str = new StringBuilder("");
            str.Append("SELECT * FROM ");
            str.Append(this.TableName);
            str.Append(" WHERE ");

            var props = model.GetType().GetProperties();

            int count = 0;
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (p.GetValue(model) != null)
                    {
                        if (count >= 1)
                        {
                            str.Append(" AND ");
                        }
                        str.Append(columnName + " = :" + p.Name);
                        count++;
                    }
                }

            }

            if (count == 0)
            {
                throw new Exception("파라미터로 전달된 모델 객체에 값이 존재하는 프로퍼티가 하나 이상 존재해야 합니다.");
            }


            this.WriteLog("GetItemStr query", str.ToString(), JsonConvert.SerializeObject(model));
            return str;
        }
        public T GetItem<T>(T model)
        {
            var item = connection.QuerySingleOrDefault<T>(GetItemStr(model).ToString(), model);

            return item;
        }


        public async Task<T> GetItemAsync<T>(T model)
        {
            var item = await connection.QuerySingleOrDefaultAsync<T>(GetItemStr(model).ToString(), model);

            return item;
        }


        public StringBuilder InsertStr<T>(T model)
        {
            var props = model.GetType().GetProperties();

            long count = 0;

            StringBuilder str = new StringBuilder("");

            str.Append("insert into ");
            str.Append(this.TableName);
            str.Append(" ( ");
            foreach (var p in props)
            {
                if (!this.CheckIgnore(p))
                {
                    var columnName = this.ColumnName(p);

                    if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                    {
                        if (p.GetValue(model) != null || this.CheckCreatedDate(p))
                        {
                            if (count >= 1)
                            {
                                str.Append(" , ");
                            }
                            str.Append(columnName);
                            count++;
                        }
                    }
                }

            }

            str.Append(" ) ");

            str.Append(" values ");
            str.Append(" ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!this.CheckIgnore(p))
                {
                    var columnName = this.ColumnName(p);

                    if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                    {

                        if (p.GetValue(model) != null || this.CheckCreatedDate(p))
                        {
                            if (count >= 1)
                            {
                                str.Append(" , ");
                            }

                            if (this.CheckCreatedDate(p))
                            {
                                str.Append("sysdate");
                            }
                            else
                            {
                                str.Append(":" + p.Name);
                            }


                            count++;
                        }
                    }
                }
            }
            str.Append(" ) ");

            Trace.TraceError("Insert Query", str.ToString());
            this.WriteLog("Insert Query", str.ToString(), JsonConvert.SerializeObject(model));

            return str;
        }

        public int Insert()
        {
            return Insert(this.Model); ;
        }

        public int Insert(object model)
        {
            try
            {


                var result = connection.Execute(InsertStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<int> InsertAsync()
        {
            return await InsertAsync(this.Model); ;
        }

        public async Task<int> InsertAsync(object model)
        {
            try
            {

                var result = await connection.ExecuteAsync(InsertStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private StringBuilder UpdateStr<T>(T model)
        {
            var props = model.GetType().GetProperties();

            long count = 0;

            StringBuilder str = new StringBuilder("");

            str.Append("update ");
            str.Append(this.TableName);
            str.Append(" set ");
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (p.GetValue(model) != null || this.CheckLastModifiedDate(p))
                    {
                        if (!this.CheckIgnore(p))
                        {
                            if (count >= 1)
                            {
                                str.Append(" , ");
                            }

                            if (this.CheckLastModifiedDate(p))
                            {
                                str.Append(columnName + " = sysdate");
                            }
                            else
                            {
                                str.Append(columnName + " = :" + p.Name);
                            }
                            count++;
                        }
                    }
                }

            }


            str.Append(" where ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (this.CheckKey(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(" and ");
                        }
                        str.Append(columnName + " = :" + p.Name);
                        count++;
                    }
                }

            }

            Trace.TraceError("Update Query", str.ToString(), JsonConvert.SerializeObject(model));

            return str;
        }

        public int Update()
        {
            return Update(this.Model); ;
        }

        public int Update(object model)
        {
            try
            {
                var result = connection.Execute(UpdateStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<int> UpdateAsync()
        {
            return await UpdateAsync(this.Model); ;
        }

        public async Task<int> UpdateAsync(object model)
        {
            try
            {
                var result = await connection.ExecuteAsync(UpdateStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private StringBuilder MergeStr<T>(T model)
        {
            var props = model.GetType().GetProperties();

            long count = 0;

            StringBuilder str = new StringBuilder("");

            str.Append("MERGE INTO ");
            str.Append(this.TableName);
            str.Append(" USING DUAL ON ( ");

            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (this.CheckKey(p))
                    {
                        if (count >= 1)
                        {
                            str.Append(" and ");
                        }
                        str.Append(columnName + " = :" + p.Name);
                        count++;
                    }
                }

            }


            str.Append(" )  WHEN MATCHED THEN UPDATE SET ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (p.GetValue(model) != null || this.CheckLastModifiedDate(p))
                    {
                        if (!this.CheckIgnore(p))
                        {
                            if (!this.CheckKey(p))
                            {
                                if (count >= 1)
                                {
                                    str.Append(" , ");
                                }

                                if (this.CheckLastModifiedDate(p))
                                {
                                    str.Append(columnName + " = sysdate");
                                }
                                else
                                {
                                    str.Append(columnName + " = :" + p.Name);
                                }

                                count++;
                            }
                        }
                    }
                }

            }

            str.Append(" WHEN NOT MATCHED THEN INSERT ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!this.CheckIgnore(p))
                {
                    var columnName = this.ColumnName(p);

                    if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                    {
                        if (p.GetValue(model) != null || this.CheckCreatedDate(p))
                        {
                            if (count >= 1)
                            {
                                str.Append(" , ");
                            }
                            str.Append(columnName);
                            count++;
                        }
                    }
                }

            }

            str.Append(" ) ");

            str.Append(" values ");
            str.Append(" ( ");

            count = 0;
            foreach (var p in props)
            {
                if (!this.CheckIgnore(p))
                {
                    var columnName = this.ColumnName(p);

                    if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                    {
                        if (p.GetValue(model) != null || this.CheckCreatedDate(p))
                        {
                            if (count >= 1)
                            {
                                str.Append(" , ");
                            }

                            if (this.CheckCreatedDate(p))
                            {
                                str.Append("sysdate");
                            }
                            else
                            {
                                str.Append(":" + p.Name);
                            }

                            count++;
                        }
                    }
                }
            }
            str.Append(" ) ");

            Trace.TraceError("Merge Query", str.ToString(), JsonConvert.SerializeObject(model));

            return str;
        }

        public int Merge()
        {
            return Merge(this.Model); ;
        }

        public int Merge(object model)
        {
            try
            {
                var result = connection.Execute(MergeStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> MergeAsync()
        {
            return await MergeAsync(this.Model); ;
        }

        public async Task<int> MergeAsync(object model)
        {
            try
            {
                var result = await connection.ExecuteAsync(MergeStr(model).ToString(), model);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private StringBuilder DeleteStr<T>(T model)
        {
            var props = model.GetType().GetProperties();

            long count = 0;

            StringBuilder str = new StringBuilder("");

            str.Append("delete from ");
            str.Append(this.TableName);



            str.Append(" where ");

            count = 0;
            foreach (var p in props)
            {
                var columnName = this.ColumnName(p);

                if (this.ColumnList.Exists(x => x.COLUMN_NAME == columnName))
                {
                    if (p.GetValue(model) != null)
                    {
                        if (count >= 1)
                        {
                            str.Append(" and ");
                        }
                        str.Append(columnName + " = :" + p.Name);
                        count++;
                    }

                }

            }



            Trace.TraceError("Delete Query", str.ToString());
            this.WriteLog("Delete Query", str.ToString(), JsonConvert.SerializeObject(model));

            return str;
        }


        public int Delete(object model)
        {
            try
            {
                var result = connection.Execute(DeleteStr(model).ToString(), model);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<int> DeleteAsync(object model)
        {
            try
            {
                var result = await connection.ExecuteAsync(DeleteStr(model).ToString(), model);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public void WriteLog(string title, string desc, string param)
        {

            this.logger.Debug(" {title} : {desc} / param : {param}", title, desc, param);

        }



        private void GetColumnList(string tableName)
        {
            string sql = @"SELECT COLUMN_NAME
                            FROM ALL_TAB_COLUMNS
                            where
                              table_name = :tableName";

            this.ColumnList = connection.Query<ColumnInfo>(sql, new { tableName = tableName }).ToList();


        }


        private string GetTableName(Type classType)
        {
            string tblName = "";

            var attrs = classType.GetCustomAttributes(true);

            foreach (var attr in attrs)
            {
                if (attr is Table)
                {
                    Table tbl = (Table)attr;
                    tblName = tbl.GetName();
                }

            }
            return tblName;
        }

        private string GetTableSynonym(Type classType)
        {
            string tblName = "";

            var attrs = classType.GetCustomAttributes(true);

            foreach (var attr in attrs)
            {
                if (attr is Table)
                {
                    Table tbl = (Table)attr;
                    tblName = tbl.GetSynonym();
                }

            }
            return tblName;
        }


        private bool CheckKey(PropertyInfo propInfo)
        {
            bool isOK = false;

            var attrs = propInfo.GetCustomAttributes(true);
            var name = propInfo.Name;

            foreach (var attr in attrs)
            {
                if (attr is KeyColumn)
                {
                    isOK = true;
                }

            }
            return isOK;
        }

        private string ColumnName(PropertyInfo p)
        {
            var columnName = p.Name;

            var attrs = p.GetCustomAttributes(true);

            foreach (var attr in attrs)
            {
                if (attr is BindingColumn)
                {
                    BindingColumn a = (BindingColumn)attr;
                    columnName = a.getName();
                }

            }

            return columnName;
        }


        private bool CheckIgnore(PropertyInfo propInfo)
        {
            bool isOK = false;

            var attrs = propInfo.GetCustomAttributes(true);
            var name = propInfo.Name;

            foreach (var attr in attrs)
            {
                if (attr is IgnoreColumn)
                {
                    isOK = true;
                }

            }
            return isOK;
        }


        private bool CheckCreatedDate(PropertyInfo propInfo)
        {
            bool isOK = false;

            var attrs = propInfo.GetCustomAttributes(true);
            var name = propInfo.Name;

            foreach (var attr in attrs)
            {
                if (attr is CreatedDate)
                {
                    isOK = true;
                }

            }
            return isOK;
        }

        private bool CheckLastModifiedDate(PropertyInfo propInfo)
        {
            bool isOK = false;

            var attrs = propInfo.GetCustomAttributes(true);
            var name = propInfo.Name;

            foreach (var attr in attrs)
            {
                if (attr is LastModifiedDate)
                {
                    isOK = true;
                }

            }
            return isOK;
        }

        public class ColumnInfo
        {
            public string COLUMN_NAME { get; set; }
        }




    }

    #region Attributes

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

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class Table : System.Attribute
    {
        private string name;
        private string synonym;

        public Table(string name)
        {
            this.name = name;
            this.synonym = "";
        }

        public Table(string name, string synonym)
        {
            this.name = name;
            this.synonym = synonym;
        }

        public string GetName()
        {
            return this.name;
        }

        public string GetSynonym()
        {
            return this.synonym;
        }
    }

    #endregion

    public interface ILogger
    {
        void Log(string message);
        void Log(string msgType, string message);
    }


    public class DebugLogger : ILogger
    {
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
        public void Log(string msgType, string message)
        {
            Debug.WriteLine(msgType + " : " + message);
        }
    }
}