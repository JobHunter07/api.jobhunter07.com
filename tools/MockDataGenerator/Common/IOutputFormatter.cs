using System;

public interface IOutputFormatter
{
    void Header(string text);
    void Info(string text);
    void Success(string text);
    void Progress(string text);
}
