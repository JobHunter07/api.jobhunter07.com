using System;
using System.IO;

public class FileProvider : IFileProvider
{
    public string ResolveOutputPath(string requestedOutput)
    {
        if (Path.IsPathRooted(requestedOutput))
            return requestedOutput;

        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        return Path.Combine(projectDir, requestedOutput);
    }

    public void EnsureDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path) ?? Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
        Directory.CreateDirectory(dir);
    }
}
