using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace stockAnalysis
{
    [Serializable]
    public class Criteria
    {
        public string Name { get; set; }
        public List<Pre> pre { get; set; }
        public string agKey { get; set; }
        public string agSum { get; set; }
        public List<Post> post { get; set; }

    }

    public class Pre
    {
        public string column { get; set; }
        public string process { get; set; }
        public List<string> values { get; set; }
    }


    public class Post
    {
        public string column { get; set; }
        public string process { get; set; }
        public List<string> values { get; set; }

    }
}
