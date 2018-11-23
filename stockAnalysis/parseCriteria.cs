using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace stockAnalysis
{
    class parseCriteria
    {

        public static List<Criteria> ParseCriteria()
        {
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

            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Ryan\Documents\stockAnalysis\stockAnalysis\Criteria sets.txt");
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
                            list.Add(criteria);
                        }
                    }
                }

            }

            return list;
        }
    }
}
