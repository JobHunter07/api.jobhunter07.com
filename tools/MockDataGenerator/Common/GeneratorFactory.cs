using System.Collections.Generic;
using System.Linq;

public class GeneratorFactory : IGeneratorFactory
{
    private readonly Dictionary<string, IGenerator> _map;

    public GeneratorFactory(IEnumerable<IGenerator> generators)
    {
        _map = generators?.ToDictionary(g => g.Id, g => g, System.StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, IGenerator>();
    }

    public IEnumerable<IGenerator> GetAll() => _map.Values;

    public IGenerator? GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _map.TryGetValue(id, out var g) ? g : null;
    }
}
