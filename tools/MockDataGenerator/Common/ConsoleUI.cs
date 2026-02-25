using Spectre.Console;

public static class ConsoleUI
{
    public static void Header(string title)
    {
        // Big header using figlet and a rule
        var fig = new FigletText(title).Centered();
        AnsiConsole.Write(fig);
        AnsiConsole.Write(new Rule(title).RuleStyle(Style.Parse("cyan")));
    }

    public static string ReadInput(string label, string? defaultValue)
    {
        var prompt = new TextPrompt<string>($"[yellow]{label}[/]")
            .DefaultValue(defaultValue ?? string.Empty)
            .AllowEmpty();

        return AnsiConsole.Prompt(prompt) ?? string.Empty;
    }

    public static void Info(string text)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(text)}[/]");
    }

    public static void Success(string text)
    {
        AnsiConsole.MarkupLine($"[bold green]✔ {Markup.Escape(text)}[/]");
    }

    public static void Progress(string text)
    {
        AnsiConsole.MarkupLine($"[yellow]> {Markup.Escape(text)}[/]");
    }

    public static void Error(string text)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(text)}[/]");
    }

    public static void WriteOptions(IEnumerable<string> options)
    {
        foreach (var opt in options)
            AnsiConsole.MarkupLine(Markup.Escape(opt));
    }
}
