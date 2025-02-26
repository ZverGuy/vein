namespace vein;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using compilation;
using ishtar;
using MoreLinq;
using syntax;
using static Spectre.Console.AnsiConsole;

public class CompilationState
{
    public Queue<CompilationEventData> warnings { get; } = new();
    public Queue<CompilationEventData> errors { get; } = new();
    public Queue<CompilationEventData> infos { get; } = new();
}


[ExcludeFromCodeCoverage]
public static class Log
{
    public static CompilationState State = new CompilationState();

    public static class Defer
    {
        public static void Warn(string text, BaseSyntax posed, DocumentDeclaration doc)
            => _print(text, posed, doc, State.warnings);
        public static void Error(string text, BaseSyntax posed, DocumentDeclaration doc)
            => _print(text, posed, doc, State.errors);
        public static void Info(string text, BaseSyntax posed, DocumentDeclaration doc)
            => _print(text, posed, doc, State.infos);

    }

    public static void EnqueueErrorsRange(IEnumerable<CompilationEventData> s) => s.Pipe(x => State.errors.Enqueue(x)).Consume();
    public static void EnqueueInfosRange(IEnumerable<CompilationEventData> s) => s.Pipe(x => State.infos.Enqueue(x)).Consume();
    public static void EnqueueWarnsRange(IEnumerable<CompilationEventData> s) => s.Pipe(x => State.warnings.Enqueue(x)).Consume();


    public static void Info(string s) => MarkupLine($"[aqua]INFO[/]: {s}");
    public static void Warn(string s) => MarkupLine($"[orange]WARN[/]: {s}");
    public static void Error(string s) => MarkupLine($"[red]ERROR[/]: {s}");
    public static void Error(Exception s) => WriteException(s);

    public static void Info(string s, CompilationTarget t) => t.Logs.Info.Enqueue($"[aqua]INFO[/]: {s}");
    public static void Warn(string s, CompilationTarget t) => t.Logs.Warn.Enqueue($"[orange]WARN[/]: {s}");
    public static void Error(string s, CompilationTarget t) => t.Logs.Error.Enqueue($"[red]ERROR[/]: {s}");

    private static void _print(string text, BaseSyntax posed, DocumentDeclaration doc, Queue<CompilationEventData> queue)
    {
        if (posed is { Transform: null })
        {
            State.errors.Enqueue(new CompilationEventData(doc, posed, text));
            return;
        }

        var strBuilder = new StringBuilder();

        strBuilder.Append($"{text.EscapeArgumentSymbols()}\n");



        if (posed is not null)
        {
            strBuilder.Append(
                $"\tat '[orange bold]{posed.Transform.pos.Line} line, {posed.Transform.pos.Column} column[/]' \n");
        }

        if (doc is not null)
        {
            strBuilder.Append(
                $"\tin '[orange bold]{doc}[/]'.");
        }

        if (posed is not null && doc is not null)
        {
            var diff_err = posed.Transform.DiffErrorFull(doc);
            strBuilder.Append($"\t\t{diff_err}");
        }

        queue.Enqueue(new CompilationEventData(doc, posed, strBuilder.ToString()));
    }
}
