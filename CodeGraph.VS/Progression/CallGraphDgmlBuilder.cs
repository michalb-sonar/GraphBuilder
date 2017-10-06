using System;
using System.Linq;
using CodeGraph.Interfaces;
using Microsoft.VisualStudio.Diagrams.View;
using Microsoft.VisualStudio.GraphModel;

namespace CodeGraph.VS.Progression
{
    internal class CallGraphDgmlBuilder
    {
        public static Graph Create(ICallGraph callGraph)
        {
            CallGraphDgmlBuilder builder = new CallGraphDgmlBuilder();
            Graph g = builder.CreateDiagram(callGraph);

            LayoutGraph(g);

            return g;
        }
        
        private static void LayoutGraph(Graph g)
        {
            using (UndoableGraphTransactionScope transactionScope = new UndoableGraphTransactionScope("LayoutGraphLeftToRight"))
            {
                foreach (GraphGroup gg in g.Groups)
                {
                    gg.SetLayoutSettings(GroupLayoutStyle.LeftToRight);
                }
                transactionScope.Complete();
            }
        }

        private CallGraphDgmlBuilder()
        {
            // private constructor
        }

        private ObjectMapper<object, GraphObject> mapper;
        private Graph graph;

        private Graph CreateDiagram(ICallGraph model)
        {
            //if (model == null)
            //{
            //    return null;
            //}

            this.mapper = new ObjectMapper<object, GraphObject>();

            this.graph = DeadCodeSchema.CreateGraph();

            foreach(Node method in model.Nodes)
            {
                ProcessNode(method);
            }

            return this.graph;
        }

        private GraphNode ProcessNode(Node method)
        {
            GraphNode gn;
            gn = this.mapper.TryGetTarget<GraphNode>(method);
            if (gn != null)
            {
                return gn; // already processed
            }

            gn = CreateMethodNode(method);

            foreach (Node caller in method.Incoming)
            {
                GraphNode gnCaller = ProcessNode(method);
                if (gnCaller != null)
                {
                    CreateZombieCallLink(gnCaller, gn);
                }
            }

            foreach (Node called in method?.Outgoing)
            {
                GraphNode gnCalled = ProcessNode(method);
                if (gnCalled != null)
                {
                    CreateZombieCallLink(gn, gnCalled);
                }
            }

            return gn;
        }

        private GraphNode CreateMethodNode(Node method)
        {
            string id = method.Symbol.ToDisplayString(Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat);

            // Create and register
            GraphNode gn = this.graph.Nodes.CreateNew(id);
            this.mapper.Register(method, gn);

            // TODO: Set properties

            if (method.Incoming?.Any() == true)
            {
                gn.AddCategory(DeadCodeSchema.NodeCategories.DeadMethodCategory);
            }
            else
            {
                gn.AddCategory(DeadCodeSchema.NodeCategories.ZombieMethodCategory);
            }

            return gn;
        }

        private void CreateZombieCallLink(GraphNode source, GraphNode target)
        {
            GraphLink link = this.graph.Links.GetOrCreate(source, target);
            link.AddCategory(DeadCodeSchema.LinkCategories.ZombieCallLinkCategory);
        }
    }
}
