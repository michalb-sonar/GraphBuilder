using System.Collections.Generic;
using CodeGraph.Interfaces;

namespace CodeGraph.VS.Tests.Helpers
{
    internal class MockCallGraph : ICallGraph
    {
        public MockCallGraph()
        {
            this.Nodes = new List<Node>();
        }

        public List<Node> Nodes { get; set; }

    }
}
