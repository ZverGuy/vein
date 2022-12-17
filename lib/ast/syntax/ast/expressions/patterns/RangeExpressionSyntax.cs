namespace vein.syntax;

using Sprache;

public class RangeExpressionSyntax : ExpressionSyntax, IPositionAware<RangeExpressionSyntax>
{
    public ExpressionSyntax S1, S2;

    public RangeExpressionSyntax(IOption<ExpressionSyntax> e1, IOption<ExpressionSyntax> e2)
    {
        this.S1 = e1.GetOrDefault();
        this.S2 = e2.GetOrDefault();
    }
    public RangeExpressionSyntax(ExpressionSyntax e1, ExpressionSyntax e2)
    {
        this.S1 = e1;
        this.S2 = e2;
    }

    public new RangeExpressionSyntax SetPos(Position startPos, int length)
    {
        base.SetPos(startPos, length);
        return this;
    }
}