using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using CodeGraph.Interfaces;

namespace CATool1
{
    public class CallGraph : ICallGraph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
    }

    public class GraphBuilder
    {
        public ICallGraph Build(string projectFilePath)
        {
            //projectFilePath = @"C:\src\CodeGraph\Graph.ConsoleApp\Graph.ConsoleApp.csproj";

            var symbolToNode = new Dictionary<ISymbol, Node>();
            var ws = MSBuildWorkspace.Create();
            Project project = ws.OpenProjectAsync(projectFilePath).Result;
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

                //Dump(m, project.Solution);
                //DumpRefs(m, project.Solution);
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


    class Program
    {
        static void Main(string[] args)
        {


        }


        private static void Dump(ISymbol called, Solution solution)
        {
            Console.WriteLine("");
            Console.WriteLine($"Method {called.Name} is called by:");

            IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(called, solution).Result;
            foreach (SymbolCallerInfo caller in callers)
            {
                Console.WriteLine($"    {caller.CallingSymbol.ContainingType.Name}.{caller.CallingSymbol.Name}");
            }

        }


        //*******************************************************





        private static void DumpRefs(ISymbol caller, Solution solution)
        {
            Console.WriteLine("");
            Console.WriteLine($"Method {caller.Name} references:");

            IEnumerable<ReferencedSymbol> refs = SymbolFinder.FindReferencesAsync(caller, solution).Result;
            foreach (ReferencedSymbol refd in refs)
            {
                Console.WriteLine($"    {refd.Definition.GetType().Name} {refd.Definition.Name}");
            }
        }
    }
}
