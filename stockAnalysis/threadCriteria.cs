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
            var list = client.CreateDocumentQuery<Criteria>(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"))
                                        .ToList();

            //number of logical processors
            //Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);

            ////number of cores
            //int coreCount = 0;
            //foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            //{
            //    coreCount += int.Parse(item["NumberOfCores"].ToString());
            //}
            //Console.WriteLine("Number Of Cores: {0}", coreCount);

            ////number of physical processors
            //foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            //{
            //    Console.WriteLine("Number Of Physical Processors: {0} ", item["NumberOfProcessors"]);
            //}

            var cb = new SqlConnectionStringBuilder();
            cb.DataSource = "tcp:cis625.database.windows.net,1433";
            cb.UserID = "admin123";
            cb.Password = "Nimda123";
            cb.InitialCatalog = "625data";
            cb.MultipleActiveResultSets = true;
            SqlConnection myConnection = new SqlConnection(cb.ConnectionString);
            myConnection.Open();


            //forEach(criteria, do this to current criteria)
            Parallel.ForEach(list, (currentCriteria) =>
            {
                //Thread.Sleep(1000); // used to slow it down until actual code is implemented, to make sure it utilizes multiple threads


                //if(currentCriteria.Name =="CriteriaSet1")
                Plinkq(currentCriteria, dt,myConnection);

               //Console.WriteLine("Processing {0} on thread {1}", currentCriteria, Thread.CurrentThread.ManagedThreadId);//Check to see what threads it is using
            });
            //Console.WriteLine("done");
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
                            pre.process == "=" ? row.Field<string>(pre.column).ToUpper() == value.ToUpper() : (pre.process == "IN" ? row.Field<string>(pre.column).ToUpper().Contains(value.ToUpper()) : (pre.process == "<>" ? row.Field<string>(pre.column).ToUpper() != value.ToUpper() : throw new Exception("Pre-agg process not reconized")))

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

            //printDataTable.PrintTable(aggregatedTable);
            
            

            //var news = resu.GroupBy(x => new NTuple<object>(from column in columnsToGroupBy select x[column])).Select(val => val.First());//new NTuple<object>(from sum in sumsToSelect select val[sum])
            Console.WriteLine(aggregatedTable);
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

            Console.WriteLine(dataFromSQL);
            var addFound = currentData.Clone();
            foreach (DataRow curRows in currentData.Rows)
            {

                dataFromSQL.PrimaryKey = new DataColumn[] { dataFromSQL.Columns["AggKey"] };
                string key = curRows.Field<string>("AggregatedKey");
                decimal max;
                var valueBroke = new string[criteria.post.Count];
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
                                    valueBroke[criteria.post.IndexOf(name)] =value;

                                }
                                else if (Convert.ToDecimal(curRows.Field<string>(name.column)) < Convert.ToDecimal(value) && Convert.ToDecimal(sqlRow.Field<string>(name.column)) > Convert.ToDecimal(value))
                                {
                                    //crosses
                                    valueBroke[criteria.post.IndexOf(name)] = value;

                                }

                                else if(Convert.ToDecimal(curRows.Field<string>(name.column)) == Convert.ToDecimal(value) && Convert.ToDecimal(sqlRow.Field<string>(name.column)) != Convert.ToDecimal(curRows.Field<string>(name.column)))
                                {
                                    //crosses
                                    valueBroke[criteria.post.IndexOf(name)] = value;

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
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) > Convert.ToDecimal(sqlRow.Field<string>(name.column)))
                                {
                                    if (Convert.ToDecimal(sqlRow.Field<string>(name.column)) < Convert.ToDecimal(value) && Convert.ToDecimal(curRows.Field<string>(name.column))> Convert.ToDecimal(value)) {
                                        //Passes Max
                                        valueBroke[criteria.post.IndexOf(name)] = value;

                                    }
                                    max = Convert.ToDecimal(curRows.Field<string>(name.column));
                                }
                                else
                                {
                                    //not pass max
                                }
                            }

                        }
                        else if (name.process == ">")
                        {
                            foreach (var value in name.values) {
                                if (Convert.ToDecimal(curRows.Field<string>(name.column)) > Convert.ToDecimal(value))
                                {
                                    //Passes > 
                                    valueBroke[criteria.post.IndexOf(name)] = value;
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
                                    valueBroke[criteria.post.IndexOf(name)] = value;
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
                                    valueBroke[criteria.post.IndexOf(name)] = value;
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
                                    valueBroke[criteria.post.IndexOf(name)] = value;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Post agg process not known");
                        }
                    }

                }
                if (valueBroke.FirstOrDefault() != null)
                {
                    addFound.Columns.Add("threshold");
                    curRows.Table.Columns.Add("threshold");
                    curRows["threshold"] = valueBroke.FirstOrDefault();
                    addFound.ImportRow(curRows);
                    foreach (var value in valueBroke)
                        Console.WriteLine("Criteria " + criteria.post[Array.IndexOf(valueBroke, value)].process + " " + value);

                }

            }
            var printMe = addFound.Copy();
            writeCSV(printMe, null, "PostAggDetails", criteria.Name);
            printMe = addFound.Copy();
            var list = criteria.agSum.Split(',').ToList();
            list.Add("AggregatedKey");
            writeCSV(printMe, list, "PostAggFiltered", criteria.Name);
            printMe = addFound.Copy();

            Console.WriteLine(printMe);



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
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }



            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(path + "\\" + "ThresholdOutput" + criteria.Name + ".csv", sb.ToString());


        }



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
            File.WriteAllText(path + "\\" + step+ criteriaName + ".csv", sb.ToString());


        }


    }

    
    
}
