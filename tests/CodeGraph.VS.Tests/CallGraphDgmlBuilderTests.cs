using System;
using CodeGraph.Interfaces;
using CodeGraph.VS.Progression;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGraph.VS.Tests
{
    [TestClass]
    public class CallGraphDgmlBuilderTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            ICallGraph callGraph = null;
            Graph g = CallGraphDgmlBuilder.Create(callGraph);
        }
    }
}
