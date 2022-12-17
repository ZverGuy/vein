namespace vein.syntax;

using System;
using System.Collections.Generic;
using Sprache;

public class StatementSyntax : ExpressionSyntax, IAdvancedPositionAware<StatementSyntax>
{
    public StatementSyntax() { }

    public override SyntaxType Kind => SyntaxType.Statement;

    public override IEnumerable<BaseSyntax> ChildNodes => NoChildren;

    public new StatementSyntax SetPos(Position startPos, int length)
    {
        base.SetPos(startPos, length);
        return this;
    }

    public Position StartPoint { get; set; }
    public Position EndPoint { get; set; }

    public StatementSyntax SetStart(Position startPos)
    {
        StartPoint = startPos;
        return this;
    }

    public StatementSyntax SetEnd(Position endPos)
    {
        EndPoint = endPos;
        return this;
    }

    public StatementSyntax SetStart(ExpressionSyntax s)
    {
        if (s.Transform is null)
            throw new InvalidOperationException(
                $"[IAdvancedPositionAware::SetStart] '{s.GetType().Name}' has incorrect transform position.");

        StartPoint = s.Transform.pos;
        return this;
    }

    public StatementSyntax SetEnd(ExpressionSyntax s)
    {
        if (s.Transform is null)
            throw new InvalidOperationException(
                $"[IAdvancedPositionAware::SetEnd] '{s.GetType().Name}' has incorrect transform position.");

        EndPoint = s.Transform.pos;
        return this;
    }

    public bool IsInside(Position t)
    {
        if (EndPoint is null)
            return false;
        if (StartPoint is null)
            return false;
        return t.Line >= StartPoint.Line && t.Line <= EndPoint.Line;
    }
}