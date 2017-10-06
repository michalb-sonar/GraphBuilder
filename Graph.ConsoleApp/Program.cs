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
using CodeGraph;

namespace CATool1
{
    class Program
    {
        static void Main(string[] args)
        {
            var graphBuilder = new GraphBuilder();
            graphBuilder.Build(@"C:\src\CodeGraph\Graph.ConsoleApp\Graph.ConsoleApp.csproj");
            //graphBuilder.Build(@"C:\src\CodeGraph\CodeGraph.Interfaces\CodeGraph.Interfaces.csproj");
        }

        private void A() { }
        private void B() { A(); }
        private void C() { B(); }


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
