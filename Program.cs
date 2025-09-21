using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityCodeAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            string outputFile = args.Length > 1 ? args[1] : "unity_code_structure.csv";

            Console.WriteLine($"Analyzing Unity project at: {projectPath}");

            var results = new List<CodeMember>();
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !IsExcludedPath(f))
                .ToList();

            Console.WriteLine($"Found {csFiles.Count} C# files");

            foreach (var filePath in csFiles)
            {
                try
                {
                    Console.WriteLine($"Processing: {Path.GetFileName(filePath)}");
                    var relativePath = Path.GetRelativePath(projectPath, filePath);
                    results.AddRange(AnalyzeFile(filePath, relativePath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }

            WriteToCsv(results, outputFile);
            Console.WriteLine($"\nAnalysis complete! Results saved to {outputFile}");
            Console.WriteLine($"Total entries: {results.Count}");
        }

        static bool IsExcludedPath(string path)
        {
            var excludedDirs = new[] { "Library", "Temp", "obj", "bin", ".vs", ".git" };
            return excludedDirs.Any(dir => path.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar) ||
                                          path.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar));
        }

        static List<CodeMember> AnalyzeFile(string filePath, string relativePath)
        {
            var results = new List<CodeMember>();
            var code = File.ReadAllText(filePath);
            
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var walker = new CodeAnalysisWalker(relativePath);
            walker.Visit(root);
            
            return walker.Results;
        }

        static void WriteToCsv(List<CodeMember> results, string outputFile)
        {
            using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);
            
            writer.WriteLine("FilePath,ClassName,MemberName,MemberType");
            
            foreach (var result in results)
            {
                writer.WriteLine($"\"{result.FilePath}\",\"{result.ClassName}\",\"{result.MemberName}\",\"{result.MemberType}\"");
            }
        }
    }

    public class CodeMember
    {
        public string FilePath { get; set; }
        public string ClassName { get; set; }
        public string MemberName { get; set; }
        public string MemberType { get; set; }
    }

    public class CodeAnalysisWalker : CSharpSyntaxWalker
    {
        public List<CodeMember> Results { get; } = new List<CodeMember>();
        private readonly string _filePath;
        private string _currentClass = "";

        public CodeAnalysisWalker(string filePath)
        {
            _filePath = filePath;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var className = node.Identifier.ValueText;
            var previousClass = _currentClass;
            _currentClass = className;

            Results.Add(new CodeMember
            {
                FilePath = _filePath,
                ClassName = className,
                MemberName = className,
                MemberType = "Class"
            });

            base.VisitClassDeclaration(node);
            
            _currentClass = previousClass;
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var structName = node.Identifier.ValueText;
            var previousClass = _currentClass;
            _currentClass = structName;

            Results.Add(new CodeMember
            {
                FilePath = _filePath,
                ClassName = structName,
                MemberName = structName,
                MemberType = "Struct"
            });

            base.VisitStructDeclaration(node);
            
            _currentClass = previousClass;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!string.IsNullOrEmpty(_currentClass))
            {
                Results.Add(new CodeMember
                {
                    FilePath = _filePath,
                    ClassName = _currentClass,
                    MemberName = node.Identifier.ValueText,
                    MemberType = "Method"
                });
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!string.IsNullOrEmpty(_currentClass))
            {
                Results.Add(new CodeMember
                {
                    FilePath = _filePath,
                    ClassName = _currentClass,
                    MemberName = node.Identifier.ValueText,
                    MemberType = "Property"
                });
            }

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (!string.IsNullOrEmpty(_currentClass))
            {
                foreach (var variable in node.Declaration.Variables)
                {
                    Results.Add(new CodeMember
                    {
                        FilePath = _filePath,
                        ClassName = _currentClass,
                        MemberName = variable.Identifier.ValueText,
                        MemberType = "Field"
                    });
                }
            }

            base.VisitFieldDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (!string.IsNullOrEmpty(_currentClass))
            {
                Results.Add(new CodeMember
                {
                    FilePath = _filePath,
                    ClassName = _currentClass,
                    MemberName = node.Identifier.ValueText,
                    MemberType = "Constructor"
                });
            }

            base.VisitConstructorDeclaration(node);
        }
    }
}