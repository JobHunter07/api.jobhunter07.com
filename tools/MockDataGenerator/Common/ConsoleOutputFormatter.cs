using System;

public class ConsoleOutputFormatter : IOutputFormatter
{
    public void Header(string text)
    {
        ConsoleUI.Header(text);
    }

    public void Info(string text)
    {
        ConsoleUI.Info(text);
    }

    public void Success(string text)
    {
        ConsoleUI.Success(text);
    }

    public void Progress(string text)
    {
        ConsoleUI.Progress(text);
    }
}
