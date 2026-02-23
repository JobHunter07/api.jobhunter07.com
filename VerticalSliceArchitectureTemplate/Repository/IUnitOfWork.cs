using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerticalSliceArchitectureTemplate.Repository
{
    public interface IUnitOfWork
    {

        int Commit();

        void Rollback();

        Task<int> CommitAsync(CancellationToken cancellationToken);

        Task RollbackAsync();
    }
}
