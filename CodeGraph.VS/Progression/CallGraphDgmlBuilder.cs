using System;
using System.Linq;
using CodeGraph.Interfaces;
using Microsoft.VisualStudio.Diagrams.View;
using Microsoft.VisualStudio.GraphModel;

namespace CodeGraph.VS.Progression
{
    public class CallGraphDgmlBuilder
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
                GraphNode gnCaller = ProcessNode(caller);
                if (gnCaller != null)
                {
                    CreateZombieCallLink(gnCaller, gn);
                }
            }

            foreach (Node called in method?.Outgoing)
            {
                GraphNode gnCalled = ProcessNode(called);
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

            string label = method.Symbol.ContainingType.Name + ":" + method.Symbol.Name;

            GraphNode gn = this.graph.Nodes.CreateNew(id);
            gn.Label = label;

            this.mapper.Register(method, gn);

            // TODO: Set properties
            
            switch(method.State)
            {
                case State.Dead:
                    gn.AddCategory(DeadCodeSchema.NodeCategories.DeadMethodCategory);
                    break;
                case State.Zombie:
                    gn.AddCategory(DeadCodeSchema.NodeCategories.ZombieMethodCategory);
                    break;
                default:
                    break;
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
