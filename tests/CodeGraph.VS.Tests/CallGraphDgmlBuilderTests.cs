using System;
using System.IO;
using CodeGraph;
using CodeGraph.Interfaces;
using CodeGraph.VS.Progression;
using CodeGraph.VS.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGraph.VS.Tests
{
    [TestClass]
    public class CallGraphDgmlBuilderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CreateSimpleGraph()
        {
            string source = @"

class TestClass1
{

    public void DoStuff()
    {
        Called1();
    }

    private void Called1() {}

    private void NotCalled1() {}

}
";

            Project project = RoslynUtilities.CreateProjectFromSource(source);

            GraphBuilder codeGraphBuilder = new GraphBuilder();
            
            ICallGraph callGraph = codeGraphBuilder.Build(project);

            Graph g = CallGraphDgmlBuilder.Create(callGraph);

            SaveToFile(g);

        }

        private void SaveToFile(Graph graph)
        {
            string filePath = Path.Combine(this.TestContext.ResultsDirectory, TestContext.TestName + ".dgml");
            graph.Save(filePath);

            this.TestContext.AddResultFile(filePath);
        }

        private Node CreateNode()
        {
            Node n = new Node();
            return n;
        }
    }
}
