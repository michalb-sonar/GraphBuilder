using CodeGraph.Interfaces;
using Microsoft.VisualStudio.GraphModel;

namespace CodeGraph.VS
{
    internal class CallGraphDgmlBuilder
    {
        public static Graph Create(ICallGraph callGraph)
        {
            CallGraphDgmlBuilder builder = new CallGraphDgmlBuilder();
            Graph g = builder.CreateDiagram(callGraph);
            return g;
        }
        
        private CallGraphDgmlBuilder()
        {
            // private constructor
        }

        protected Graph CreateDiagram(ICallGraph model)
        {
            //if (model == null)
            //{
            //    return null;
            //}

            Graph graph = this.CreateGraph();
            return graph;
        }

        private Graph CreateGraph()
        {
            // TODO: schema
            Graph g = new Graph();
            return g;
        }

    }
}
