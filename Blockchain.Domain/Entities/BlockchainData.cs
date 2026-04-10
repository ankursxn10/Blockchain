using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Domain.Entities
{
    public class BlockchainData
    {
        public int Id { get; set; }
        public string BlockchainType { get; set; }
        public string JsonData { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
