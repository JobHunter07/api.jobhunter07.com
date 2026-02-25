public interface IMenu
{
    // Returns (generatorId or null for quit, requestedOutput, count, compress)
    (string? generatorId, string requestedOutput, int count, bool compress) GetSelection(string[] args, System.Collections.Generic.IEnumerable<IGenerator> generators);
}
