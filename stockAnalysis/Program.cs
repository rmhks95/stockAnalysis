using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stockAnalysis
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            parseCriteria();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            
        }


        static void parseCriteria()
        {
            string line;
            int colCounter = 0;
            int CriteriaCount = -1;
            string agKey = "";
            string agSum = "";
            string [] preProcess = new string[10];
            string[] preAg = new string[10];
            string[,] preValues = new string[10, 15];
            string[] postProcess = new string[10];
            string[] postAg = new string[10];
            string[,] postValues = new string[10, 15];
            Criteria criteria = new Criteria();
            List<Criteria> list = new List<Criteria>();

            System.IO.StreamReader file = new System.IO.StreamReader(@"U:\\stockAnalysis\\stockAnalysis\\criteria sets.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("--"))
                {
                    if (line.StartsWith("!"))
                    {
                        criteria = new Criteria();
                        criteria.Name = line.Substring(1);
                        CriteriaCount = -1;
                        agKey = "";
                        preAg = new string[10];
                        postAg = new string[10];
                    }
                    else if (line.StartsWith("@"))
                    {
                        CriteriaCount++;
                        preAg[CriteriaCount] = line.Substring(1);
                        colCounter = 0;
                    }
                    else if (line.StartsWith("^"))
                    {
                        preProcess[CriteriaCount] = line.Substring(1);
                    }
                    else if (line.StartsWith("#"))
                    {
                        if (Regex.IsMatch(line, @"[A-Za-z]"))
                        {
                            preValues[CriteriaCount, colCounter] = line.Substring(1);
                            colCounter++;
                        }
                        if (Regex.IsMatch(line, @"\d"))
                        {
                            postValues[CriteriaCount, colCounter] = line.Substring(1);
                            colCounter++;
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
                        CriteriaCount = -1;
                    }
                    else if (line.StartsWith("$"))
                    {
                        CriteriaCount++;
                        postAg[CriteriaCount] = line.Substring(1);
                        colCounter = 0;
                    }
                    else if (line.StartsWith("&"))
                    {
                        postProcess[CriteriaCount] = line.Substring(1);
                    }

                    if (string.IsNullOrEmpty(line) && file.ReadLine().StartsWith("--") || file.EndOfStream)
                    {
                        if (criteria.Name != null)
                        {
                            criteria.PreAgCol = preAg;
                            criteria.PreProcess = preProcess;
                            criteria.PreValues = preValues;
                            criteria.agKey = agKey.Substring(0, agKey.Length - 1);
                            criteria.agSum = agSum.Substring(0, agSum.Length - 1);
                            criteria.PostAgCol = postAg;
                            criteria.PostProcess = postProcess;
                            criteria.PostValues = postValues;
                            list.Add(criteria);
                        }
                    }
                }

            }

        }
    }
}
