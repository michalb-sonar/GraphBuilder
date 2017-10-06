using System.Collections.Generic;
using System.Linq;
using CodeGraph.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace CodeGraph
{
    public class GraphBuilder
    {
        public ICallGraph Build(string projectFilePath)
        {
            //projectFilePath = @"C:\src\CodeGraph\Graph.ConsoleApp\Graph.ConsoleApp.csproj";
            var ws = MSBuildWorkspace.Create();
            Project project = ws.OpenProjectAsync(projectFilePath).Result;

            return Build(project);
        }

        public ICallGraph Build(Project project)
        {

            var symbolToNode = new Dictionary<ISymbol, Node>();
            Compilation compilation = project.GetCompilationAsync().Result;

            var memberSymbols = new List<ISymbol>();
            CollectMembers(compilation.GlobalNamespace, memberSymbols);

            memberSymbols = memberSymbols
                .Where(m => m.DeclaringSyntaxReferences.Any())
                .ToList();

            foreach (ISymbol m in memberSymbols)
            {
                symbolToNode[m] = new Node { Symbol = m };
            }

            foreach (ISymbol m in memberSymbols)
            {
                var thisSymbolNode = symbolToNode[m];
                IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(m, project.Solution).Result;

                foreach (var callerNode in callers.Select(c => symbolToNode[c.CallingSymbol]))
                {
                    thisSymbolNode.Incoming.Add(callerNode);
                    callerNode.Outgoing.Add(thisSymbolNode);
                }
            }

            return new CallGraph { Nodes = symbolToNode.Select(kvp => kvp.Value).ToList() };
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

        private static void CollectMembers(INamespaceSymbol namespaceSymbol, IList<ISymbol> collectedMembers)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                CollectMembers(type, collectedMembers);
            }

            foreach (var childNs in namespaceSymbol.GetNamespaceMembers())
            {
                CollectMembers(childNs, collectedMembers);
            }
        }

        private static void CollectMembers(INamedTypeSymbol type, IList<ISymbol> collectedMembers)
        {
            foreach (var member in type.GetMembers())
            {
                if (member.CanBeReferencedByName)
                {
                    collectedMembers.Add(member);
                }
            }

            foreach (var nested in type.GetTypeMembers())
            {
                CollectMembers(nested, collectedMembers);
            }
        }
    }
}
