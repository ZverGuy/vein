namespace vein.syntax;

using System.Linq.Expressions;
using Sprache;

public partial class VeinSyntax
{
    #region Wrappers

    private Parser<ExpressionSyntax> WrappedExpression(char open, char close) =>
        (from ob in Parse.Char(open).Token()
            from cb in Parse.Char(close).Token()
            select new ExpressionSyntax($"{ob}{cb}")).Token().Positioned();
    private Parser<T> WrappedExpression<T>(char open, char close, Parser<T> parserUnit) =>
        (from ob in Parse.Char(open).Token()
            from t in parserUnit.Token()
            from cb in Parse.Char(close).Token()
            select t).Token();

    #endregion

    #region BinaryExp helpers

    private ExpressionSyntax FlatIfEmptyOrNull(ExpressionSyntax exp, IOption<ExpressionSyntax> option, string op)
    {
        if (option.IsDefined)
            return new BinaryExpressionSyntax(exp, option.Get(), op);
        return exp;
    }
    private ExpressionSyntax FlatIfEmptyOrNull(ExpressionSyntax exp, CoalescingExpressionSyntax coalescing)
    {
        if (coalescing is not { First: null, Second: null })
            return new BinaryExpressionSyntax(exp, coalescing, "?:");
        return exp;
    }
    private ExpressionSyntax FlatIfEmptyOrNull<T>(ExpressionSyntax exp, (string op, T exp)[] data) where T : ExpressionSyntax
    {
        if (data.Length == 0)
            return exp;
        if (data.Length == 1)
            return SimplifyOptimization(new BinaryExpressionSyntax(exp, data[0].exp, data[0].op));
        var e = exp;

        foreach (var (op, newExp) in data)
        {
            e = SimplifyOptimization(new BinaryExpressionSyntax(e, newExp, op));
        }

        return e;
    }

    #endregion


    #region exp_simplify_optimize

    private ExpressionSyntax SimplifyOptimization(BinaryExpressionSyntax binary)
    {
        if (!AppFlags.HasFlag(ApplicationFlag.exp_simplify_optimize))
            return binary;

        if (binary is not { Left: { Kind: SyntaxType.LiteralExpression }, Right: { Kind: SyntaxType.LiteralExpression } })
            return binary;

        var l1 = binary.Left;
        var l2 = binary.Right;


        if (l1.GetType() != l2.GetType())
            return binary;
        try
        {
            if (l1 is UndefinedIntegerNumericLiteral n1 && l2 is UndefinedIntegerNumericLiteral n2)
            {
                var v1 = long.Parse(n1.Value);
                var v2 = long.Parse(n2.Value);

                switch (binary.OperatorType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return new UndefinedIntegerNumericLiteral($"{v1 + v2}");
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return new UndefinedIntegerNumericLiteral($"{v1 - v2}");
                }
            }
        }
        catch { }
        return binary;
    }

    #endregion

}