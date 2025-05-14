using Azure;
using Azure.Data.Tables;
using System;

namespace API.Models
{
    public class ProductEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public string FarmerId { get; set; } = string.Empty;
    }
}
