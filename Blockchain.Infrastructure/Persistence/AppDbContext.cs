using Blockchain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<BlockchainData> BlockchainData { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}
