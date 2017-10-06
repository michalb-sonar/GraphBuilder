using System.Collections.Generic;
using System.Linq;
using CodeGraph.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System.Threading;

namespace CodeGraph
{
    public class GraphBuilder
    {
        public ICallGraph Build(string projectFilePath)
        {
            var ws = MSBuildWorkspace.Create();
            Project project = ws.OpenProjectAsync(projectFilePath).Result;

            return Build(project);
        }

        public ICallGraph Build(Project project)
        {
            var symbolToNode = new Dictionary<ISymbol, Node>();
            Compilation compilation = project.GetCompilationAsync().Result;

            var memberSymbols = new List<ISymbol>();
            CollectMembers(compilation.GlobalNamespace, memberSymbols, compilation);

            foreach (ISymbol m in memberSymbols)
            {
                symbolToNode[m] = new Node { Symbol = m };
            }

            foreach (ISymbol m in memberSymbols)
            {
                var thisSymbolNode = symbolToNode[m];
                IEnumerable<SymbolCallerInfo> callerInfos = SymbolFinder.FindCallersAsync(m, project.Solution).Result;
                var callers = callerInfos
                                .Select(ci => ci.CallingSymbol)
                                .Where(HasDeclaringSyntax)
                                .ToList();

                foreach (var caller in callers)
                {
                    Node callerNode;

                    if (symbolToNode.TryGetValue(caller, out callerNode))
                    {
                        thisSymbolNode.Incoming.Add(callerNode);
                        callerNode.Outgoing.Add(thisSymbolNode);
                    }
                    else
                    {
                        // TODO: ?create dummy caller node to represent caller?
                        thisSymbolNode.State = State.Live;
                    }
                }
            }

            var graph = new CallGraph { Nodes = symbolToNode.Select(kvp => kvp.Value).ToList() };
            AnnotateDead(compilation, graph);
            return graph;
        }

        private static bool HasDeclaringSyntax(ISymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences.Any();
        }

        public void AnnotateDead(Compilation compilation, ICallGraph graph)
        {
            var nodesToVisit = new Queue<Node>();

            foreach (var node in graph.Nodes.Where(n => n.Symbol.IsPubliclyAccessible() || n.State == State.Live))
            {
                nodesToVisit.Enqueue(node);
            }

            var mainMethod = compilation.GetEntryPoint(CancellationToken.None);
            if (mainMethod != null)
            {
                nodesToVisit.Enqueue(graph.Nodes.Where(n => n.Symbol.Equals(mainMethod)));
            }

            while (nodesToVisit.Count > 0)
            {
                var node = nodesToVisit.Dequeue();
                if (node.State != State.Live)
                {
                    node.State = State.Live;
                    nodesToVisit.Enqueue(node.Outgoing);
                }
            }

            foreach (var node in graph.Nodes
                .Where(n => n.State != State.Live))
            {
                node.State = node.Incoming.Count == 0
                    ? State.Dead
                    : State.Zombie;
            }
        }

        private static void CollectMethodSymbols(ITypeSymbol typeSymbol, IList<IMethodSymbol> collectedMethods)
        {
            if (!typeSymbol.DeclaringSyntaxReferences.Any())
            {
                return;
            }

            foreach (IMethodSymbol m in typeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                collectedMethods.Add(m);
            }

            foreach (ITypeSymbol t in typeSymbol.GetTypeMembers())
            {
                CollectMethodSymbols(t, collectedMethods);
            }
        }

        private static void CollectMembers(INamespaceSymbol namespaceSymbol, IList<ISymbol> collectedMembers,
            Compilation compilation)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                CollectMembers(type, collectedMembers, compilation);
            }

            foreach (var childNs in namespaceSymbol.GetNamespaceMembers())
            {
                CollectMembers(childNs, collectedMembers, compilation);
            }
        }

        private static void CollectMembers(INamedTypeSymbol type, IList<ISymbol> collectedMembers,
            Compilation compilation)
        {
            foreach (var member in type.GetMembers())
            {
                if (member.CanBeReferencedByName &&
                    HasDeclaringSyntax(member) &&
                    member.ContainingAssembly == compilation.Assembly)
                {
                    collectedMembers.Add(member);
                }
            }

            foreach (var nested in type.GetTypeMembers())
            {
                CollectMembers(nested, collectedMembers, compilation);
            }
        }
    }

    public static class Utils
    {
        public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
        {
            if (symbol == null)
            {
                return Accessibility.NotApplicable;
            }

            var result = symbol.DeclaredAccessibility;
            if (result == Accessibility.Private)
            {
                return Accessibility.Private;
            }

            for (var container = symbol.ContainingType; container != null; container = container.ContainingType)
            {
                if (container.DeclaredAccessibility == Accessibility.Private)
                {
                    return Accessibility.Private;
                }

                if (container.DeclaredAccessibility == Accessibility.Internal)
                {
                    result = Accessibility.Internal;
                    continue;
                }
            }

            return result;
        }

        public static bool IsPubliclyAccessible(this ISymbol symbol)
        {
            var effectiveAccessibility = GetEffectiveAccessibility(symbol);

            return effectiveAccessibility == Accessibility.Public ||
                effectiveAccessibility == Accessibility.Protected ||
                effectiveAccessibility == Accessibility.ProtectedOrInternal;
        }

        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }
        }
    }
}
