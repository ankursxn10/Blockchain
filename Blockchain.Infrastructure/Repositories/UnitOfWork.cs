using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
