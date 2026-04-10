using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
