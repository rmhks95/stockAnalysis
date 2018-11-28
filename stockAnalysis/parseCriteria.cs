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

namespace stockAnalysis
{
    class parseCriteria
    {
        private const string EndpointUrl = "https://criteria.documents.azure.com:443/";
        private const string PrimaryKey = "eDNWOyfslnhfiiRjoUufC6ADHfcQwgXpB0e5sRCFil35hK4kwy2qU0LtSvBjuqm7BMqE2rt4xcWsOfxl2LrFPw==";
        private DocumentClient client;


        private void GetStartedDemo()
        {
            

            ParseCriteria();
        }


        public static void Info()
        {
            // ADD THIS PART TO YOUR CODE
            try
            {
                parseCriteria p = new parseCriteria();
                p.GetStartedDemo();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                //Console.ReadKey();
            }
        }


        public static void ParseCriteria()
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
            List<Criteria> list = new List<Criteria>();

            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Cjoew\Google Drive\documents\college\7th Year\CIS 625\stockAnalysis\stockAnalysis\Criteria sets.txt");
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

                    if (string.IsNullOrEmpty(line) && file.ReadLine().StartsWith("--") || file.EndOfStream)
                    {
                        if (criteria.Name != null)
                        {
                            criteria.pre.Add(pre);
                            criteria.agKey = agKey.Substring(0, agKey.Length - 1);
                            criteria.agSum = agSum.Substring(0, agSum.Length - 1);
                            criteria.post.Add(post);
                            try
                            {
                                var propertiesOfUser = client.CreateDocumentQuery<Criteria>(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"))
                                        .Where(p => p.Name == criteria.Name)
                                        .ToList(); 

                                if (!(propertiesOfUser.Count > 0))
                                {
                                    Document doc = client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"), criteria).Result;
                                }
                            }catch(Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }

            }

            

            //return list;
        }
    }
}
