using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace stockAnalysis
{
    class threadCriteria
    {
        private const string EndpointUrl = "https://criteria.documents.azure.com:443/";
        private const string PrimaryKey = "eDNWOyfslnhfiiRjoUufC6ADHfcQwgXpB0e5sRCFil35hK4kwy2qU0LtSvBjuqm7BMqE2rt4xcWsOfxl2LrFPw==";
        private DocumentClient client;


       


        public static void Start(DataTable dt)
        {
            var client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            var list = client.CreateDocumentQuery<Criteria>(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets")).ToList();


            var cb = new SqlConnectionStringBuilder();
            cb.DataSource = "tcp:cis625.database.windows.net,1433";
            cb.UserID = "admin123";
            cb.Password = "Nimda123";
            cb.InitialCatalog = "625data";
            cb.MultipleActiveResultSets = true;
            SqlConnection myConnection = new SqlConnection(cb.ConnectionString);
            myConnection.Open();


            using (SqlCommand myCmd = new SqlCommand("TRUNCATE TABLE TempTable", myConnection))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }


            Parallel.ForEach(list, (currentCriteria) =>
            {
                //if(currentCriteria.Name =="82: Criteria Set 81(Short)")
                Plinkq(currentCriteria, dt,myConnection);

            });
            myConnection.Close();
        }

        static void Plinkq(Criteria currentCriteria, DataTable dt, SqlConnection myConnection)
        {
            var results = dt.AsEnumerable();
            EnumerableRowCollection<DataRow> resu;
            List<EnumerableRowCollection> res = new List<EnumerableRowCollection>();
            List<EnumerableRowCollection> info = new List<EnumerableRowCollection>();

            DataTable table = new DataTable();

            foreach (DataColumn col in dt.Columns)
            {
                table.Columns.Add(col.ColumnName, col.DataType);
            }


            foreach (var pre in currentCriteria.pre)
            {
                DataTable temp = new DataTable();

                foreach (DataColumn col in dt.Columns)
                {
                    temp.Columns.Add(col.ColumnName, col.DataType);
                }

                foreach (var value in pre.values)
                {
                    resu = results.Where(row =>
                            pre.process == "=" || pre.process == "IN" ? row.Field<string>(pre.column).ToUpper() == value.ToUpper() : (pre.process == "<>" ? row.Field<string>(pre.column).ToUpper() != value.ToUpper() : throw new Exception("Pre-agg process not reconized"))

                         );
        
                    foreach(DataRow row in resu)
                    {
                        temp.Rows.Add(row.ItemArray);
                    }
                }
                table = temp.Copy();
                results = table.AsEnumerable();
            }
            resu = table.AsEnumerable();
            

            DataTable aggregatedTable = aggregation.Aggregate(resu, currentCriteria);
            if (aggregatedTable.Rows.Count == 0) return;

            postAgg(aggregatedTable, currentCriteria,myConnection);
        }

        static void postAgg(DataTable currentData, Criteria criteria, SqlConnection myConnection)
        {

            DataTable dataFromSQL = new DataTable();

                        
            string oString = "Select * from Stocks.RunningData where CriteriaSet='"+criteria.Name+"'";
            SqlCommand oCmd = new SqlCommand(oString, myConnection);


            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(oCmd);
            // this will query your database and return the result to your datatable
            da.Fill(dataFromSQL);
               
            da.Dispose();

            var addFound = currentData.Clone();
            currentData.Columns.Add("Criteria");
            currentData.Columns.Add("SharesHeldPastMax");
            currentData.Columns.Add("PercentageSharesHeldPastMax");
            currentData.Columns.Add("ValuePastMax");
            foreach (DataRow curRows in currentData.Rows)
            {
                //curRows["Criteria"] = criteria.Name;
                dataFromSQL.PrimaryKey = new DataColumn[] { dataFromSQL.Columns["AggKey"] };
                string key = curRows.Field<string>("AggregatedKey");
                decimal max=0;
                string valueBroke ="";
                DataRow sqlRow = dataFromSQL.Rows.Find(key);

                if (sqlRow != null)
                {

                    foreach (Post name in criteria.post)
                    {
                        if (name.process.ToUpper() == "CROSSES")
                        {
                            foreach (var value in name.values)
                            {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) > Convert.ToDecimal(value) && Convert.ToDecimal(sqlRow.Field<string>(name.column)) < Convert.ToDecimal(value))
                                {
                                    //crosses
                                    valueBroke =value;

                                }
                                else if (Convert.ToDecimal(curRows.Field<string>(name.column)) < Convert.ToDecimal(value) && Convert.ToDecimal(sqlRow.Field<string>(name.column)) > Convert.ToDecimal(value))
                                {
                                    //crosses
                                    valueBroke= value;

                                }

                                else if(Convert.ToDecimal(curRows.Field<string>(name.column)) == Convert.ToDecimal(value) && Convert.ToDecimal(sqlRow.Field<string>(name.column)) != Convert.ToDecimal(curRows.Field<string>(name.column)))
                                {
                                    //crosses
                                    valueBroke = value;

                                }
                                else
                                {
                                    //doesnt cross
                                }

                            }

                        }
                        else if (name.process.ToUpper() == "MAX")
                        {
                            foreach (var value in name.values)
                            {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) >= Convert.ToDecimal(sqlRow.Field<string>(name.column)))
                                {
                                    if (Convert.ToDecimal(sqlRow.Field<string>(name.column)) < Convert.ToDecimal(value) && Convert.ToDecimal(curRows.Field<string>(name.column))> Convert.ToDecimal(value)) {
                                        //Passes Max
                                        valueBroke = value;

                                    }
                                    curRows[name.column.Substring(0, 1).ToUpper() + name.column.Substring(1) + "PastMax"] = curRows[name.column];

                                }
                                else
                                {
                                    //not pass max
                                    curRows[name.column.Substring(0, 1).ToUpper() + name.column.Substring(1) + "PastMax"] = sqlRow[name.column];
                                }
                            }

                        }
                        else if (name.process == ">")
                        {
                            foreach (var value in name.values) {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) > Convert.ToDecimal(value))
                                {
                                    //Passes > 
                                    valueBroke = value;
                                }
                            }
                        }
                        else if (name.process == ">=")
                        {
                            foreach (var value in name.values)
                            {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) >= Convert.ToDecimal(value))
                                {
                                    //Passes >=
                                    valueBroke = value;
                                }
                            }
                        }
                        else if (name.process == "<")
                        {
                            foreach (var value in name.values)
                            {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) < Convert.ToDecimal(value))
                                {
                                    //Passes < 
                                    valueBroke = value;
                                }
                            }
                        }
                        else if (name.process == "<=")
                        {
                            foreach (var value in name.values)
                            {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) <= Convert.ToDecimal(value))
                                {
                                    //Passes <=
                                    valueBroke = value;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Post agg process not known");
                        }
                    }

                }
                if (valueBroke!="")
                {
                    if (!(addFound.Columns.Contains("threshold"))) addFound.Columns.Add("threshold");
                    if (!(curRows.Table.Columns.Contains("threshold"))) curRows.Table.Columns.Add("threshold");
                    curRows["threshold"] = valueBroke;
                    addFound.ImportRow(curRows);
                    
                }


                //insertRunningInfo(curRows, currentData, max, criteria);

                /*using (SqlBulkCopy bulkCopy = new SqlBulkCopy(myConnection))
                {
                    bulkCopy.DestinationTableName =
                        "Stocks.TempTable";

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(curRows.Table);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }*/


                oCmd = new SqlCommand(oString, myConnection);
               

            }


            //add current data currentData

            //using (myConnection)
            //{
            //StringBuilder sCommand = new StringBuilder("Insert Into stocks.temptable VALUES");
            //List<string> Rows = new List<string>();
            //foreach (DataRow row in currentData.Rows)
            var calc = currentData.Rows.Count / 1000;
                for (int j = 0; j<calc+1;j++)
                {
                    
                    StringBuilder sCommand = new StringBuilder("Insert Into stocks.temptable VALUES");
                    List<string> Rows = new List<string>();
                    for (int i = 0; i <1000; i++)
                    {
                        int counter = (j * 1000) + i;
                        if (currentData.Rows.Count <= counter) { break; }

                        if (currentData.Rows[counter][3].ToString().Contains("'"))
                            currentData.Rows[counter][3] = currentData.Rows[counter][3].ToString().Replace("'", "''");

                        Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')", currentData.Rows[counter][0], currentData.Rows[counter][1], currentData.Rows[counter][2], currentData.Rows[counter][3], currentData.Rows[counter][4], currentData.Rows[counter][5], currentData.Rows[counter][6], currentData.Rows[counter][7], currentData.Rows[counter][8], criteria.Name, currentData.Rows[counter][10], currentData.Rows[counter][11], currentData.Rows[counter][12]));
                        
                    }
                    if (Rows.Count != 0) {
                        sCommand.Append(string.Join(",", Rows));
                        sCommand.Append(";");
                        //myConnection.Open();
                        using (SqlCommand myCmd = new SqlCommand(sCommand.ToString(), myConnection))
                        {
                            myCmd.CommandType = CommandType.Text;
                            myCmd.ExecuteNonQuery();
                        }
                    }
                }


                //SqlBulkCopy bulkCopy = new SqlBulkCopy(myConnection);
                //bulkCopy.ColumnMappings.Add("stockcode", "StockCode");
                //bulkCopy.ColumnMappings.Add("stocktype", "StockType");
                //bulkCopy.ColumnMappings.Add("holderid", "HolderID");
                //bulkCopy.ColumnMappings.Add("holdercountry", "HolderCountry");
                //bulkCopy.ColumnMappings.Add("sharesheld", "SharesHeld");
                //bulkCopy.ColumnMappings.Add("percentagesharesheld", "PercentageSharesHeld");
                //bulkCopy.ColumnMappings.Add("direction", "Direction");
                //bulkCopy.ColumnMappings.Add("value", "Value");
                //bulkCopy.ColumnMappings.Add("Criteria", "CriteriaSet");
                //bulkCopy.ColumnMappings.Add("AggregatedKey", "AggKey");
                //bulkCopy.ColumnMappings.Add("SharesHeldPastMax", "SharesHeldPastMax");
                //bulkCopy.ColumnMappings.Add("PercentageSharesHeldPastMax", "PercentageSharesHeldPastMax");
                //bulkCopy.ColumnMappings.Add("ValuePastMax", "ValuePastMax");

                //bulkCopy.DestinationTableName = "stocks.temptable";

                //bulkCopy.WriteToServer(currentData);






                //}





                var printMe = addFound.Copy();
            writeCSV(printMe, null, "PostAggDetails", criteria.Name);
            printMe = addFound.Copy();
            var list = criteria.agSum.Split(',').ToList();
            list.Add("AggregatedKey");
            writeCSV(printMe, list, "PostAggFiltered", criteria.Name);
            printMe = addFound.Copy();

            StringBuilder sb = new StringBuilder();


            sb.AppendLine("Set,"+criteria.Name);
            sb.AppendLine("Date," + DateTime.Now.ToString("MM/dd/yyyy"));
            
            foreach(var key in criteria.agKey.Split(','))
            {
                sb.AppendLine("Agg Key Column," + key);
            }


            var columns = new List<string>();
            foreach (var post in criteria.post)
            {
                sb.AppendLine("Post Agg Column," + post.column);

                columns.Add(post.column);
                
            }

            var removeRows = new List<string>();
            columns.Add("AggregatedKey");
            columns.Add("threshold");
            foreach (DataColumn column in printMe.Columns)
            {
                if (!(columns.Contains(column.ColumnName))) removeRows.Add(column.ColumnName);

            }

            foreach (var name in removeRows)
            {
                printMe.Columns.Remove(name);
            }

            sb.AppendLine("");
            sb.AppendLine("ColumnValue,Agg Key,Threshold Broke");



            foreach (DataRow row in printMe.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => { field = field.ToString().Replace(",", ""); return field.ToString(); });
                sb.AppendLine(string.Join(",", fields));
            }



            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string criteriaNameFix = criteria.Name.Contains(':') ? criteria.Name.Split(':').First() : criteria.Name;
            File.WriteAllText(path + "\\results\\" + "ThresholdOutput" + criteriaNameFix + ".csv", sb.ToString());


        }


        /*static void insertRunningInfo(DataRow curRows, DataTable currentData, decimal max, Criteria criteria)
        {
            var cb = new SqlConnectionStringBuilder();
            DataTable dataCheck = new DataTable();
            cb.DataSource = "tcp:cis625.database.windows.net,1433";
            cb.UserID = "admin123";
            cb.Password = "Nimda123";
            cb.InitialCatalog = "625data";

            using (SqlConnection dbConnection = new SqlConnection(cb.ConnectionString))
            {
               

                string oString = "Select * from Stocks.RunningData where CriteriaSet='" + criteria.Name + "'and AggKey='"+curRows["AggregatedKey"] +"'";
                SqlCommand oCmd = new SqlCommand(oString, dbConnection);


                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(oCmd);
                // this will query your database and return the result to your datatable
                da.Fill(dataCheck);
                
                da.Dispose();
            }

            string q;


            if (dataCheck.Rows.Count < 1)
            {
                q = "INSERT into stocks.runningdata (AggKey, CriteriaSet, StockCode, StockType, HolderID, HolderCountry, SharesHeld, SharesHeldPastMax, PercentageSharesHeld, PercentageSharesHeldPastMax, Direction, Value, ValuePastMax) " +
                 "VALUES('" + curRows["AggregatedKey"] + "','" + criteria.Name + "','" + curRows["stockcode"] + "','" + curRows["stocktype"] + "','" + curRows["holderid"] + "','" + curRows["holdercountry"] + "','" + curRows["sharesheld"] + "','" + curRows["sharesheld"] + "','" + curRows["percentagesharesheld"] + "','" + curRows["percentagesharesheld"] + "','" + curRows["direction"] + "','" + curRows["value"] + "','" + curRows["value"] + "')";
            }
            else
            {
                q = "Update Stocks.RunningData Set ";
                foreach (DataColumn col in currentData.Columns)
                    if (col.ColumnName != "AggregatedKey" && col.ColumnName!="threshold")
                    {
                        if (curRows[col.ColumnName].ToString().Contains("'")) curRows[col.ColumnName].ToString().Replace("'", "''");
                        q += curRows[col].GetType() == curRows["AggregatedKey"].GetType() ? col + "='" + curRows[col] + "'," : "";
                        try
                        {
                            if (col.ColumnName == "value" || col.ColumnName == "sharesheld" || col.ColumnName == "percentagesharesheld")
                                if (curRows[col.ColumnName].GetType() == curRows["AggregatedKey"].GetType())
                                    if (max == Convert.ToDecimal(curRows[col.ColumnName]))
                                        q += col + "PastMax='" + curRows[col.ColumnName] + "',";
                        }
                        catch
                        {

                        }
                    }

                q = q.Substring(0, q.Length - 1);
                q += "where criteriaSet='" + criteria.Name + "' and AggKey='" + curRows["AggregatedKey"] + "'";

            }

            using (SqlConnection dbConnection = new SqlConnection(cb.ConnectionString))
            {
                dbConnection.Open();

               
                
                SqlCommand cmd = new SqlCommand(q, dbConnection);
                cmd.ExecuteNonQuery();
                dbConnection.Close();
            }
            

        }*/


        static void writeCSV(DataTable currentData, List<string> columns, string step, string criteriaName)
        {
            StringBuilder sb = new StringBuilder();

            if (columns != null)
            {
                var removeRows = new List<string>();
                foreach(DataColumn column in currentData.Columns)
                {
                    if (!(columns.Contains(column.ColumnName))) removeRows.Add(column.ColumnName);

                }
                foreach(var name in removeRows)
                {
                    currentData.Columns.Remove(name);
                }

            }
            IEnumerable<string> columnNames = currentData.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in currentData.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            


            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string criteriaNameFix = criteriaName.Contains(':') ? criteriaName.Split(':').First() : criteriaName;
            File.WriteAllText(path + "\\results\\" + step+ criteriaNameFix + ".csv", sb.ToString());


        }


    }

    
    
}
