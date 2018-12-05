using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;

namespace stockAnalysis
{
    class parseCriteria
    {
        private const string EndpointUrl = "https://criteria.documents.azure.com:443/";
        private const string PrimaryKey = "eDNWOyfslnhfiiRjoUufC6ADHfcQwgXpB0e5sRCFil35hK4kwy2qU0LtSvBjuqm7BMqE2rt4xcWsOfxl2LrFPw==";
        private DocumentClient client;


        public static void ParseCriteria(string path)
        {
            var client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            string line;
            int colCounter = 0;
            int entrieCount = 0;
            string agKey = "";
            string agSum = "";
            Criteria criteria = new Criteria();
            Pre pre = new Pre();
            Post post = new Post();
            int index = 0;

            System.IO.StreamReader file = new System.IO.StreamReader(path); 
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("--"))
                {
                    if (line.StartsWith("!"))
                    {
                        criteria = new Criteria();
                        pre = new Pre();
                        post = new Post();
                        criteria.Name = line.Substring(1);
                        colCounter = 0;
                        entrieCount = 0;
                        agKey = "";
                        agSum = "";
                    }
                    else if (line.StartsWith("@"))
                    {
                        if (criteria.pre == null) criteria.pre = new List<Pre>();
                        if (pre.column != null) criteria.pre.Add(pre);
                        pre = new Pre();
                        pre.column = line.Substring(1);

                    }
                    else if (line.StartsWith("^"))
                    {

                        pre.process = line.Substring(1);
                    }
                    else if (line.StartsWith("#"))
                    {
                        if (Regex.IsMatch(line, @"[A-Za-z]"))
                        {
                            if (pre.values == null) pre.values = new List<string>();
                            pre.values.Add(line.Substring(1));
                        }
                        if (Regex.IsMatch(line, @"\d"))
                        {
                            if (post.values == null) post.values = new List<string>();
                            post.values.Add(line.Substring(1));
                        }
                    }
                    else if (line.StartsWith("*"))
                    {
                        agKey += line.Substring(1);
                        agKey += ",";
                    }
                    else if (line.StartsWith("+"))
                    {
                        agSum += line.Substring(1);
                        agSum += ",";
                        entrieCount = 0;
                    }
                    else if (line.StartsWith("$"))
                    {

                        if (criteria.post == null) criteria.post = new List<Post>();
                        if (post.column != null) criteria.post.Add(post);
                        post = new Post();
                        post.column = line.Substring(1);
                    }
                    else if (line.StartsWith("&"))
                    {
                        post.process = line.Substring(1);
                    }

                    if (file.EndOfStream || (string.IsNullOrEmpty(line)&& (!string.IsNullOrEmpty(criteria.Name))))
                    {
                        criteria.pre.Add(pre);
                        criteria.agKey = agKey.Substring(0, agKey.Length - 1);
                        criteria.agSum = agSum.Substring(0, agSum.Length - 1);
                        criteria.post.Add(post);
                        try
                        {
                            var cb = new SqlConnectionStringBuilder();
                            cb.DataSource = "tcp:cis625.database.windows.net,1433";
                            cb.UserID = "admin123";
                            cb.Password = "Nimda123";
                            cb.InitialCatalog = "625data";

                            var propertiesOfUser = client.CreateDocumentQuery<Criteria>(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"))
                                  .Where(p => p.Name == criteria.Name)
                                  .ToList();

                            if (!(propertiesOfUser.Count > 0))
                            {
                                using (SqlConnection dbConnection = new SqlConnection(cb.ConnectionString))
                                {
                                    dbConnection.Open();
                                    string query = string.Format("INSERT INTO CriteriaFiltering.CriteriaSets(Name) Values('{0}')", criteria.Name);
                                    SqlCommand cmd = new SqlCommand(query, dbConnection);
                                    cmd.ExecuteNonQuery();
                                    dbConnection.Close();
                                }

                                Document doc = client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"), criteria).Result;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }

            }
        }
    }
}
