using System;
using CodeGraph.Interfaces;
using CodeGraph.VS.Progression;
using CodeGraph.VS.Tests.Helpers;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGraph.VS.Tests
{
    [TestClass]
    public class CallGraphDgmlBuilderTests
    {
        [TestMethod]
        public void CreateSimpleGraph()
        {
            ICallGraph callGraph = new MockCallGraph();

            Node n1 = new Node();
            Node n2 = new Node();

            n1.Incoming = new System.Collections.Generic.List<Node>()
            {
                n2
            };

            n2.Outgoing = new System.Collections.Generic.List<Node>() { n1 };

            callGraph.Nodes.Add(n1);
            callGraph.Nodes.Add(n2);

            Graph g = CallGraphDgmlBuilder.Create(callGraph);
        }

        private Node CreateNode()
        {
            Node n = new Node();
            return n;
        }
    }
}
