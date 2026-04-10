using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Infrastructure.External
{
    public interface IBlockchainService
    {
        Task<string> FetchDataAsync(string type);
    }
}
