using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace stockAnalysis
{
    class csvHandler
    {
        public static DataTable GetDataTabletFromCSVFile()
        {
            DataTable csvData = new DataTable();
            try
            {
                //C:\Users\Erik Homewood\Desktop\cis\CIS 625\project\inputs\File0.cvs
                using (TextFieldParser csvReader = new TextFieldParser(@"C:\Users\Erik Homewood\Source\Repos\stockAnalysis\stockAnalysis\File0.csv"))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return csvData;
        }


        //public static void InsertDataIntoSQLServerUsingSQLBulkCopy()
        //{
        //    var csvFileData = GetDataTabletFromCSVFile();
        //    var cb = new SqlConnectionStringBuilder();
        //    cb.DataSource = "tcp:cis625.database.windows.net,1433";
        //    cb.UserID = "admin123";
        //    cb.Password = "Nimda123";
        //    cb.InitialCatalog = "625data";


        //    using (SqlConnection dbConnection = new SqlConnection(cb.ConnectionString)) {
        //        dbConnection.Open();
        //        string query = "TRUNCATE TABLE Stocks.DailyInputs ";
        //        SqlCommand cmd = new SqlCommand(query, dbConnection);
        //        cmd.ExecuteNonQuery();
        //        using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
        //        {
        //            s.DestinationTableName = "Stocks.DailyInputs";
        //            foreach (var column in csvFileData.Columns)
        //                s.ColumnMappings.Add(column.ToString(), column.ToString());
        //            s.WriteToServer(csvFileData);
        //        }
        //    }
        //}
    }
}
