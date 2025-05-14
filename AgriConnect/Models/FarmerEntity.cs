using Azure;
using Azure.Data.Tables;
using System;

namespace AgriConnect.Models
{
    public class FarmerEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Farmer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; //stores hashed password
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}

