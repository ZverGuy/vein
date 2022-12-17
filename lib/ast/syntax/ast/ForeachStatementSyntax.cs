namespace vein.syntax;

using System.Collections.Generic;
using Sprache;

public class ForeachStatementSyntax : StatementSyntax, IPositionAware<ForeachStatementSyntax>
{
    public LocalVariableDeclaration Variable { get; }
    public ExpressionSyntax Expression { get; }
    public StatementSyntax Statement { get; }


    public override SyntaxType Kind => SyntaxType.ForEachStatement;

    public override IEnumerable<BaseSyntax> ChildNodes => new List<BaseSyntax> { Variable, Expression, Statement };

    public ForeachStatementSyntax(LocalVariableDeclaration declaration, ExpressionSyntax exp, StatementSyntax statement)
    {
        Variable = declaration;
        Expression = exp;
        Statement = statement;
    }

    public new ForeachStatementSyntax SetPos(Position startPos, int length)
    {
        base.SetPos(startPos, length);
        return this;
    }
}