﻿namespace wave.syntax
{
    using System.Linq;
    using Sprache;

    public partial class WaveSyntax
    {
        // example: now = DateTime.Now()
        protected internal virtual Parser<VariableDeclaratorSyntax> VariableDeclarator =>
           (from identifier in Identifier
               from expression in Parse.Char('=').Token().Then(_ => QualifiedExpression).Positioned().Optional()
               select new VariableDeclaratorSyntax
               {
                   Identifier = identifier,
                   Expression = ExpressionSyntax.CreateOrDefault(expression),
               }).Positioned();
        // example: int x, y, z = 3;
        protected internal virtual Parser<VariableDeclarationSyntax> VariableDeclaration =>
            (from type in TypeReference.Token().Positioned()
                from declarators in VariableDeclarator.DelimitedBy(Parse.Char(',').Token())
                from semicolon in Parse.Char(';')
                select new VariableDeclarationSyntax
                {
                    Type = type,
                    Variables = declarators.ToList(),
                }).Positioned();
    }
}