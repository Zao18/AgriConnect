using Azure;
using Azure.Data.Tables;

namespace AgriConnect.Models
{
    public class ApplicationUser : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }  // This will be the username
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // ✅ Custom properties
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
