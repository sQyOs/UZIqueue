using FirebirdSql.Data.FirebirdClient;
using System;
using System.Configuration;
using System.Data;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace UZIqueue
{

    public class RString
    {
        private string docaAddr = ConfigurationManager.AppSettings.Get("docaAddr");
        private static DateTime dateTime = new DateTime(1970, 01, 01, 11, 0, 0);
        public DateTime recordTimeMark { get; set; }
        public string barCode { get; set; }
        public string fio { get; set; }
        public DateTime dBirth { get; set; }
        public string depart { get; set; }
        public Uri url { get; }

        public RString(string _recordTimeMark, string _barCode)
        {
            if (_recordTimeMark == "")
            {
                recordTimeMark = DateTime.Now;
            }
            else
            {
                recordTimeMark = Convert.ToDateTime(_recordTimeMark);
            }
            barCode = _barCode;
            string idPat = _barCode.Substring(2, 9);
            string idHsp = _barCode.Substring(11, 9);
            connectToDoca(idHsp);
            url = new Uri($"http://{docaAddr}/docaplus/main/frame3.php?doc=Li4vYW5hbHlzeXMvYW5hbF9zdm9kX3BhdC5waHA=&id_pat={Base64Encode(idPat)}&id_hsp={Base64Encode(idHsp)}&b=1");
            //Li4vYW5hbHlzeXMvYW5hbF9zdm9kX3BhdC5waHA=
            //Li4vcGF0aWVudHMvcGF0X3ZpZXcucGhw
        }
        public DataRow GetRow(DataTable _dataTable)
        {
            Type type = this.GetType();
            DataRow row;
            row = _dataTable.NewRow();
            foreach (var item in type.GetProperties())
            {
                row[item.Name] = item.GetValue(this);
            }
            return row;
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        void connectToDoca(string idHsp)
        {

            FbConnectionStringBuilder cs = new FbConnectionStringBuilder();

            cs.DataSource = docaAddr;
            cs.Database = "/opt/firebird/database/docaplus.gdb";
            cs.UserID = "SYSDBA";
            cs.Password = "masterkey";
            cs.Charset = "UTF8";
            cs.Pooling = false;

            FbConnection connection = new FbConnection(cs.ToString());

            try //connect to db
            {
                string query = $@"select 
                                    pat_list.fam,
                                    pat_list.nam,
                                    pat_list.ots,
                                    depart.name,
                                    pat_list.d_bir
                                from dep_hsp
                                   inner join pat_list on(dep_hsp.id_pat = pat_list.id_pat)
                                   inner join depart on(dep_hsp.id_dep = depart.id)
                                   inner join hosp on(dep_hsp.id_pat = hosp.id_pat) and (dep_hsp.id_hsp = hosp.id_hsp)
                                where
                                   (
                                      (dep_hsp.id_hsp = {idHsp})
                                   )";
                connection.Open();
                //wrLine(connection.State.ToString());

                FbCommand fbCommand = new FbCommand(query, connection);
                FbDataAdapter fbDataAdapter = new FbDataAdapter(fbCommand);
                DataTable dataTable = new DataTable();
                fbDataAdapter.Fill(dataTable);
                connection.Close();
                //wrLine(connection.State.ToString());
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    Console.WriteLine(dataTable.Rows.GetEnumerator());
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Console.WriteLine(column.ColumnName + " " + dataRow[column]);
                    }
                }
                if (dataTable.Rows.Count > 0)
                {
                    string strFam = spaceKiller(dataTable.Rows[dataTable.Rows.Count - 1].ItemArray[0].ToString());
                    string strNam = spaceKiller(dataTable.Rows[dataTable.Rows.Count - 1].ItemArray[1].ToString());
                    string strOts = spaceKiller(dataTable.Rows[dataTable.Rows.Count - 1].ItemArray[2].ToString());
                    this.depart = spaceKiller(dataTable.Rows[dataTable.Rows.Count - 1].ItemArray[3].ToString());
                    this.dBirth = dateTime.AddSeconds(Convert.ToInt32(spaceKiller(dataTable.Rows[dataTable.Rows.Count - 1].ItemArray[4].ToString())));
                    this.fio = strFam + strNam + strOts;
                }
                //else
                //{
                //    wrLine($"udefined pacient");
                //    goto zapros;
                //}
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                connection.Close();
            }

            string spaceKiller(string s)
            {
                Regex regex = new Regex(@"\s+");
                return regex.Replace(s, " ");
            }

            bool availableCheck()
            {
                try
                {
                    Ping pingSender = new Ping();
                    string data = "ismyserverpingable";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = 5;
                    PingReply reply = pingSender.Send(docaAddr, timeout, buffer);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
    }
}
