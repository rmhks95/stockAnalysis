﻿using System;
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

            bool crosses = false;

            foreach (Post name in criteria.post)
            {
                if (name.process.ToUpper() == "CROSSES")
                {
                    crosses = true;
                }
                
            } 
            

            foreach (DataRow curRows in currentData.Rows)
            {
                //var rows = dataFromSQL.AsEnumerable().Where(x => x.Field<string>("AggKey").ToUpper() == curRows.Field<string>("AggregatedKey").ToUpper()).Select(s => new { things = s.Field<string>("AggKey").FirstOrDefault() });

                dataFromSQL.PrimaryKey = new DataColumn[] { dataFromSQL.Columns["AggKey"] };
                string key = curRows.Field<string>("AggregatedKey");

                DataRow rows = dataFromSQL.Rows.Find(key);

                if (false)
                {
                    //Console.WriteLine(rows.FirstOrDefault());
                    //if (curRows.Field<string>("").ToUpper() > sqlRows.Field<string>("").ToUpper())
                    //{

                    //}



                }

                

            }


        }

    }

    
    
}
