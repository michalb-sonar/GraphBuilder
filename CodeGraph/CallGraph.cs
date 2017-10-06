using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGraph.Interfaces;

namespace CodeGraph
{
    public class CallGraph : ICallGraph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
    }
}
