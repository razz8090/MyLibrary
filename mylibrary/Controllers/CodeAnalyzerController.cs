// File: CodeAnalyzerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ApiController]
[Route("api/code")]
public class CodeAnalyzerController : ControllerBase
{
    [HttpPost("flow")]
    public IActionResult AnalyzeFlow([FromBody] CodeInput input)
    {
        string userCode = input.Code;

        if (!userCode.Contains("class"))
        {
            userCode = $"public class DummyClass {{ {userCode} }}";
        }

        var tree = CSharpSyntaxTree.ParseText(userCode);
        var root = tree.GetCompilationUnitRoot();

        var nodes = new List<FlowNode>();
        var edges = new List<FlowEdge>();
        var methodMap = new Dictionary<string, string>();
        var methodParams = new Dictionary<string, List<string>>();
        var methodBodies = new Dictionary<string, BlockSyntax>();
        var methodExprBodies = new Dictionary<string, ArrowExpressionClauseSyntax>();
        int nodeId = 1;

        string AddNode(string label, string type, int level = 0, string group = null, bool isExecuted = false)
        {
            var id = (nodeId++).ToString();
            nodes.Add(new FlowNode
            {
                Id = id,
                Label = label,
                Type = type,
                Explanation = GetExplanation(type, label),
                Level = level,
                Group = group ?? "default",
                IsExecuted = isExecuted
            });
            return id;
        }

        string GetExplanation(string type, string label) => type switch
        {
            "method" => "This defines a method in the class.",
            "loop" => "This is a loop structure, executing repeatedly.",
            "condition" => "This is a conditional decision point.",
            "return" => "Returns a value from the method.",
            "call" => "Calls another method.",
            "switch" => "Switch control flow block.",
            "case" => "Handles a specific case in a switch block.",
            "try" => "Begin a try block for exception handling.",
            "catch" => "Handles exceptions thrown in try.",
            "statement" => "Executes a single statement.",
            _ => "Performs a program operation."
        };

        foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                string parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                string returnType = method.ReturnType.ToString();
                string methodId = AddNode($"Method: {method.Identifier.Text}({parameters}) → {returnType}", "method", 0, method.Identifier.Text);
                methodMap[method.Identifier.Text] = methodId;
                methodParams[method.Identifier.Text] = method.ParameterList.Parameters.Select(p => p.Identifier.Text).ToList();
                methodBodies[method.Identifier.Text] = method.Body;
                if (method.ExpressionBody != null)
                    methodExprBodies[method.Identifier.Text] = method.ExpressionBody;
            }
        }

        void TraverseStatements(IEnumerable<StatementSyntax> statements, string fromId, int level, string group = null, Dictionary<string, string> argumentMap = null)
        {
            foreach (var stmt in statements)
            {
                string currentId = fromId;

                switch (stmt)
                {
                    case LocalDeclarationStatementSyntax localDecl:
                        string declId = AddNode(localDecl.ToString(), "statement", level, group, true);
                        edges.Add(new FlowEdge { From = currentId, To = declId });
                        currentId = declId;
                        break;

                    case ForStatementSyntax forStmt:
                        string forId = AddNode($"For Loop: {forStmt.Condition}", "loop", level, group, true);
                        edges.Add(new FlowEdge { From = currentId, To = forId });
                        IEnumerable<StatementSyntax> loopStatements;
                        if (forStmt.Statement is BlockSyntax block)
                            loopStatements = block.Statements;
                        else
                            loopStatements = new List<StatementSyntax> { forStmt.Statement };
                        TraverseStatements(loopStatements, forId, level + 1, group);
                        currentId = forId;
                        break;

                    case ExpressionStatementSyntax exprStmt:
                        if (exprStmt.Expression is AssignmentExpressionSyntax assignExpr &&
                            assignExpr.Right is ObjectCreationExpressionSyntax)
                        {
                            string instanceId = AddNode(exprStmt.ToString(), "statement", level, group, true);
                            edges.Add(new FlowEdge { From = currentId, To = instanceId });
                            currentId = instanceId;
                            break;
                        }
                        goto default;

                    default:
                        TraverseGeneralStatement(stmt, ref currentId, level, group, argumentMap);
                        break;
                }
            }
        }

        void TraverseGeneralStatement(StatementSyntax stmt, ref string fromId, int level, string group, Dictionary<string, string> argumentMap)
        {
            switch (stmt)
            {
                case SwitchStatementSyntax switchStmt:
                    string switchId = AddNode($"Switch: {switchStmt.Expression}", "switch", level, group, true);
                    edges.Add(new FlowEdge { From = fromId, To = switchId });

                    string switchExpr = switchStmt.Expression.ToString();

                    foreach (var section in switchStmt.Sections)
                    {
                        foreach (var label in section.Labels)
                        {
                            string labelStr = label.ToString();
                            string labelId = AddNode($"Case: {labelStr}", "case", level + 1, group, labelStr.Contains(switchExpr));
                            edges.Add(new FlowEdge { From = switchId, To = labelId });
                            TraverseStatements(section.Statements, labelId, level + 2, group);
                        }
                    }
                    fromId = switchId;
                    break;

                default:
                    string defaultId = AddNode(stmt.ToString().Trim(), "statement", level, group, true);
                    edges.Add(new FlowEdge { From = fromId, To = defaultId });
                    fromId = defaultId;
                    break;
            }
        }

        foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                string methodId = methodMap[method.Identifier.Text];
                var statements = method.Body?.Statements ?? Enumerable.Empty<StatementSyntax>();
                TraverseStatements(statements, methodId, 1, method.Identifier.Text);
            }
        }

        return Ok(new { nodes, edges });
    }

    string TryEvaluateConstant(ExpressionSyntax expr)
    {
        try
        {
            var simpleTree = CSharpSyntaxTree.ParseText($"class Temp {{ object Temp() {{ return {expr}; }} }}");
            var model = CSharpCompilation.Create("Temp")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(simpleTree)
                .GetSemanticModel(simpleTree);

            var returnStatement = simpleTree.GetRoot()
                .DescendantNodes().OfType<ReturnStatementSyntax>().FirstOrDefault();

            if (returnStatement != null)
            {
                var constant = model.GetConstantValue(returnStatement.Expression);
                if (constant.HasValue)
                    return constant.Value?.ToString();
            }
        }
        catch { }

        return null;
    }

    IEnumerable<StatementSyntax> GetStatements(StatementSyntax stmt)
    {
        if (stmt is BlockSyntax block)
            return block.Statements;
        return new[] { stmt };
    }
}

public class CodeInput
{
    public string Code { get; set; }
}

public class FlowNode
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public int Level { get; set; } // for layout
    public string Group { get; set; } // for grouping
    public bool IsExecuted { get; set; } // for path highlighting
    public string Explanation { get; set; } // natural-language note
}

public class FlowEdge
{
    public string From { get; set; }
    public string To { get; set; }
}
