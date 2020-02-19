using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Reflection;

namespace UZIqueue
{
    class DBXML
    {
        public void SaveDataTableInXML(DataTable TableForSave)
        {
            string Name = TableForSave.TableName;
            if (Name != "")
            {
                Name += ".xml";
                StringWriter writerXML = new StringWriter();
                TableForSave.WriteXml(writerXML, XmlWriteMode.WriteSchema, true);
                File.WriteAllText(Name, writerXML.ToString());
            }
        }

        public DataTable LoadDataTableFromXML(string NameTable, Type dataClass)
        {
            DataTable ResultReadXML = null;
            if (NameTable != "")
            {
                ResultReadXML = new DataTable(NameTable);
                string fileName = NameTable + ".xml";
                if (File.Exists(fileName))
                {
                    var stringReader = new StringReader(File.ReadAllText(fileName));
                    ResultReadXML.ReadXml(stringReader);
                }
                else
                {
                    SaveDataTableInXML(CreateTable(NameTable, dataClass));
                }
            }
            return ResultReadXML;
        }
        DataTable CreateTable(string NameTable, Type dataClass)
        {
            var properties = dataClass.GetProperties();
            DataTable NewTable = new DataTable(NameTable);
            foreach (PropertyInfo info in properties)
            {
                NewTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType);
            }
            return NewTable;
        }
        //public DataTable CreateTable(string NameTable, List<DataColumn> columns)
        //{
        //    DataTable NewTable = new DataTable(NameTable);
        //    foreach (DataColumn selectInColumns in columns)
        //    {
        //        NewTable.Columns.Add(selectInColumns);
        //    }
        //    return NewTable;
        //}
        //public DataTable ConvertIEnumToDT<T>(string NameTable, IEnumerable<T> columns)
        //{
        //    Type type = typeof(T);
        //    var properties = type.GetProperties();
        //    DataTable NewTable = new DataTable(NameTable);
        //    foreach (PropertyInfo info in properties)
        //    {
        //        NewTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType);
        //    }
        //    foreach (T entity in columns)
        //    {
        //        object[] values = new object[properties.Length];
        //        for (int i = 0; i < properties.Length; i++)
        //        {
        //            values[i] = properties[i].GetValue(entity);
        //        }
        //        NewTable.Rows.Add(values);
        //    }
        //    return NewTable;
        //}

    }
}
