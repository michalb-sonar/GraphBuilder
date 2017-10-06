using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CodeGraph.VS.Tests
{
    internal static class RoslynUtilities
    {

        private static readonly MetadataReference CorLibRef = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemRef = MetadataReference.CreateFromFile(typeof(System.ComponentModel.AddingNewEventArgs).Assembly.Location);
        private static readonly MetadataReference SystemCoreRef = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference MefLibRef = MetadataReference.CreateFromFile(typeof(System.ComponentModel.Composition.CompositionError).Assembly.Location);

        public static Project CreateProjectFromSource(string source)
        {
            Project project = new AdhocWorkspace().CurrentSolution
                .AddProject("project1", "assembly1", LanguageNames.CSharp)
                .AddMetadataReference(CorLibRef)
                .AddMetadataReference(SystemRef)
                .AddMetadataReference(SystemCoreRef)
                .AddMetadataReference(MefLibRef);

            CompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            project = project.WithCompilationOptions(options);

            project = project.AddDocument("source.cs", source).Project;
            return project;
        }

        public static Compilation CreateCompilationFromSourceFiles(params string[] filePaths)
        {
            Project project = new AdhocWorkspace().CurrentSolution
                .AddProject("project1", "assembly1", LanguageNames.CSharp)
                .AddMetadataReference(CorLibRef)
                .AddMetadataReference(SystemRef)
                .AddMetadataReference(SystemCoreRef)
                .AddMetadataReference(MefLibRef);

            CompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            project = project.WithCompilationOptions(options);

            foreach(string filePath in filePaths)
            {
                string source = File.ReadAllText(filePath);
                project = project.AddDocument(Path.GetFileName(filePath), source, filePath: filePath).Project;
            }

            Compilation compilation = project.GetCompilationAsync().Result;

            CheckNoCompilationErrors(compilation);
            return compilation;
        }

        private static void CheckNoCompilationErrors(Compilation compilation)
        {
            IEnumerable<Diagnostic> errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

            if (errors.Count() > 0)
            {
                foreach(Diagnostic error in errors)
                {
                    Console.WriteLine($"{error.Id}: {error.GetMessage()}");
                }

                Debug.Fail($"Compilation errors: {errors.Count()}");
            }
        }

    }
}
