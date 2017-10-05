using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CodeGraph.Interfaces
{
    public interface ICallGraph
    {
        List<Node> Nodes { get; set; }
    }

    public class Node
    {
        public List<Node> Incoming { get; set; }

        public List<Node> Outgoing { get; set; }

        public ISymbol Symbol { get; set; }

        public Dictionary<string, string> PropertyBag { get; set; }
    }

}