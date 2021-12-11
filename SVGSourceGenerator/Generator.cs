using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SVGSourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;


            foreach (var recordSyntax in receiver.SVGElements)
            {
                var model = GetModel(recordSyntax, context.Compilation);
                if (model != null)
                {
                    var result = TemplateHelper.Render(model);

                    result = SyntaxFactory.ParseCompilationUnit(result)
                        .NormalizeWhitespace()
                        .GetText()
                        .ToString();

                    context.AddSource(model.RecordName, result);
                }
            }
        }

        private ProxyModel GetModel(RecordDeclarationSyntax recordSyntax, Compilation compilation)
        {
            var root = recordSyntax.GetCompilationUnit();
            var semanticModel = compilation.GetSemanticModel(recordSyntax.SyntaxTree);
            var typeSymbol = (ITypeSymbol)ModelExtensions.GetDeclaredSymbol(semanticModel, recordSyntax);

            if (typeSymbol is null) return null;
            if (typeSymbol.IsAbstract) return null;
            if (typeSymbol.GetBaseTypesAndThis().All(x => x.Name != "SVGElement"))
                return null;

            //Debugger.Launch();
            var members = typeSymbol.GetAllMembers().ToList();

            var proxyModel = new ProxyModel
            {
                RecordName = recordSyntax.GetRecordName(),
                Usings = root.GetUsings(),
                Namespace = root.GetNamespace(),

                SVGPropertyNames = members.Where(x => x.GetAttributes()
                        .Any(a => a.AttributeClass.Name == "SVGPropertyAttribute"))
                    .Select(x =>
                    {
                        var attribute = x.GetAttributes()
                            .First(a=>a.AttributeClass.Name =="SVGPropertyAttribute" );

                        var svgName = attribute.ConstructorArguments.First().Value?.ToString();

                        var nullable = true;
                        if (x is IPropertySymbol propertySymbol)
                        {
                            if (propertySymbol.Type.IsValueType)
                            {
                                nullable = propertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
                            }
                        }

                        return (x.Name, nullable, svgName);
                    })
                    .ToList()
            };

            return proxyModel;
        }

        
    }



    public class SyntaxReceiver : ISyntaxReceiver
    {
        public List<RecordDeclarationSyntax> SVGElements { get; } = new List<RecordDeclarationSyntax>();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is RecordDeclarationSyntax cds && IsSVGElement(cds))
            {
                SVGElements.Add(cds);
            }
        }

        static bool IsSVGElement(RecordDeclarationSyntax rds)
        {
            if (rds.BaseList != null)
            {
                foreach (var bts in rds.BaseList.Types)
                {
                    if (bts.Type.GetText().ToString() == "SVGStrokeElement")
                        return true;
                    if (bts.Type.GetText().ToString() == "SVGElement")
                        return true;
                }
            }

            return false;
        }
    }

    public class ProxyModel
    {
        public string RecordName { get; set; }
        public string Namespace { get; set; }

        public List<string> Usings { get; set; }

        public List<(string propertyName, bool nullable, string svgName)> SVGPropertyNames { get; set; }
    }

    public static class TemplateHelper
    {
        public static IEnumerable<string> ExtraUsings
        {
            get
            {
                yield return "System";
                yield return "System.Collections";
                yield return "System.Collections.Generic";
                yield return "System.Linq";
            }
        }

        public static string Render(ProxyModel proxyModel)
        {
            var sb = new StringBuilder();

            foreach (var @using in ExtraUsings.Concat(proxyModel.Usings).Distinct())
            {
                sb.AppendLine($"using {@using};");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {@proxyModel.Namespace}");

            sb.AppendLine("{");
            sb.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{nameof(SVGSourceGenerator)}\", \"1.0\")]");
            sb.AppendLine($"public sealed partial record {proxyModel.RecordName}");
            sb.AppendLine("{");
            sb.AppendLine("public override IEnumerable<(string propertyName, int index, object value)> GetProperties()");
            sb.AppendLine("{");
            var index = 1;
            foreach (var (propertyName, nullable, svgName) in proxyModel.SVGPropertyNames)
            {
                sb.AppendLine();
                if(nullable)
                    sb.AppendLine($"if({propertyName} is not null)");
                sb.AppendLine($"yield return (\"{svgName}\", {index}, {propertyName});");

                index++;
            }

            sb.AppendLine("yield break;");

            sb.AppendLine("}");


            sb.AppendLine("}");


            sb.AppendLine("}");


            return sb.ToString();
        }
    }

    public static class RoslynExtensions
    {
        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
        {
            return type.GetBaseTypesAndThis().SelectMany(n => n.GetMembers());
        }

        public static CompilationUnitSyntax GetCompilationUnit(this SyntaxNode syntaxNode)
        {
            return syntaxNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
        }

        public static string GetClassName(this ClassDeclarationSyntax proxy)
        {
            return proxy.Identifier.Text;
        }

        public static string GetRecordName(this RecordDeclarationSyntax proxy)
        {
            return proxy.Identifier.Text;
        }

        public static string GetClassModifier(this ClassDeclarationSyntax proxy)
        {
            return proxy.Modifiers.ToFullString().Trim();
        }

        public static bool HaveAttribute(this ClassDeclarationSyntax classSyntax, string attributeName)
        {
            return classSyntax.AttributeLists.Count > 0 &&
                   classSyntax.AttributeLists.SelectMany(al => al.Attributes
                           .Where(a => (a.Name as IdentifierNameSyntax).Identifier.Text == attributeName))
                       .Any();
        }


        public static string GetNamespace(this CompilationUnitSyntax root)
        {
            return root.ChildNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .Select(x => x.Name.ToString())
                .Concat(root.ChildNodes().OfType<FileScopedNamespaceDeclarationSyntax>()
                    .Select(x => x.Name.ToString())

                ).FirstOrDefault();
        }

        public static List<string> GetUsings(this CompilationUnitSyntax root)
        {
            return root.ChildNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(n => n.Name.ToString())
                .ToList();
        }
    }
}