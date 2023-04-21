using BitRex.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitRex.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<BitRex.Core.Entities.Transaction> Transactions { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
