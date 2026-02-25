using System.Collections.Generic;

public interface IGeneratorFactory
{
    IEnumerable<IGenerator> GetAll();
    IGenerator? GetById(string id);
}
