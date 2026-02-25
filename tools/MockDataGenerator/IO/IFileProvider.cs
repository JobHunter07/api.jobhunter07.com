public interface IFileProvider
{
    string ResolveOutputPath(string requestedOutput);
    void EnsureDirectory(string path);
}
