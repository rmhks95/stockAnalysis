using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stockAnalysis
{
    public class Criteria
    {
        public string Name { get; set; }
        public string[] PreAgCol { get; set; }
        public string[] PreProcess { get; set; }
        public string[,] PreValues { get; set; }
        public string agKey { get; set; }
        public string agSum { get; set; }
        public string[] PostAgCol { get; set;}
        public string[] PostProcess { get; set; }
        public string[,] PostValues { get; set; }
        //public Criteria(string name, string[] preAgCol, string preProcess, string[,] prevalues)
        //{
        //    Name = name;
        //    PreAgCol = preAgCol;
        //    PreProcess = preProcess;
        //    PreValues = PreValues;
        //}
        
            //Other properties, methods, events...
        
    }
}
