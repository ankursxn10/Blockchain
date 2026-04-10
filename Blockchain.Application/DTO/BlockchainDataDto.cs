using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Application.DTO
{
    public class BlockchainDataDto
    {
        public string BlockchainType { get; set; }
        public string JsonData { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
