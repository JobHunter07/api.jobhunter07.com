public interface ICompanyGenerator
{
    void Generate(string outputPath, int count, bool compress, System.Threading.CancellationToken cancellationToken);
}
