using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace stockAnalysis
{
    class threadCriteria
    {
        private const string EndpointUrl = "https://criteria.documents.azure.com:443/";
        private const string PrimaryKey = "eDNWOyfslnhfiiRjoUufC6ADHfcQwgXpB0e5sRCFil35hK4kwy2qU0LtSvBjuqm7BMqE2rt4xcWsOfxl2LrFPw==";
        private DocumentClient client;


        public static void Start()
        {
            var client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            var list = client.CreateDocumentQuery<Criteria>(UriFactory.CreateDocumentCollectionUri("criteria", "criteriaSets"))
                                        .ToList();

            //number of logical processors
            Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);

            //number of cores
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            Console.WriteLine("Number Of Cores: {0}", coreCount);

            //number of physical processors
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                Console.WriteLine("Number Of Physical Processors: {0} ", item["NumberOfProcessors"]);
            }

            //forEach(criteria, do this to current criteria)
            Parallel.ForEach(list, (currentCriteria) =>
            {
                Thread.Sleep(1000); // used to slow it down until actual code is implemented, to make sure it utilizes multiple threads

                Plinkq(currentCriteria);

                Console.WriteLine("Processing {0} on thread {1}", currentCriteria, Thread.CurrentThread.ManagedThreadId);//Check to see what threads it is using
            });
        }

        static void Plinkq(Criteria currentCriteria)
        {
            
        
            
        }
    }
}
