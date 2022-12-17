namespace vein.syntax;

using System;
using System.Collections.Generic;
using Sprache;

[Flags]
public enum ExpressionFlags
{
    None = 0 << 0,
    Unused = 1 << 1,
    Optimized = 1 << 2
}

public class ExpressionSyntax : BaseSyntax, IPositionAware<ExpressionSyntax>
{
    public ExpressionSyntax()
    {
    }
    public ExpressionSyntax(bool isUnused) => Flags |= ExpressionFlags.Unused;

    public ExpressionSyntax(string expr) => ExpressionString = expr;

    public override SyntaxType Kind => SyntaxType.Expression;

    public override IEnumerable<BaseSyntax> ChildNodes => NoChildren;

    public virtual string ExpressionString { get; set; }

    public bool HasUnused => Flags.HasFlag(ExpressionFlags.Unused);
    public bool HasOptimized => Flags.HasFlag(ExpressionFlags.Optimized);

    public new ExpressionSyntax SetPos(Position startPos, int length)
    {
        this.Transform = new Transform(startPos, length);
        return this;
    }

    public ExpressionSyntax Downlevel() => this;

    public ExpressionFlags Flags { get; set; } = ExpressionFlags.None;

    public ExpressionSyntax AsOptimized()
    {
        Flags |= ExpressionFlags.Optimized;
        return this;
    }

    public T As<T>() where T : ExpressionSyntax
    {
        if (this is T t)
            return t;
        throw new InvalidCastException($"Cant cast '{this.GetType().Name}' to {typeof(T).Name}.");
    }
}