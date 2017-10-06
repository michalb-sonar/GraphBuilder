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
        public List<Node> Incoming { get; set; } = new List<Node>();

        public List<Node> Outgoing { get; set; } = new List<Node>();

        public ISymbol Symbol { get; set; }

        public Dictionary<string, string> PropertyBag { get; set; } = new Dictionary<string, string>();

        public override int GetHashCode() => Symbol.GetHashCode();

        public override bool Equals(object obj) => (obj as Node)?.Symbol == Symbol;
    }
}