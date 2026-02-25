public interface IGenerator
{
    string Id { get; }
    string DisplayName { get; }
    // Generate data to the given output path. If compress is true the implementation
    // should produce a gzip-compressed output (e.g. .gz). The operation must honor
    // the provided cancellation token for graceful shutdown.
    void Generate(string outputPath, int count, bool compress, System.Threading.CancellationToken cancellationToken);
}
