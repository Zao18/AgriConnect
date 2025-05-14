using Azure;
using Azure.Data.Tables;

namespace AgriConnect.Services
{
    public class TableStorageService<T> : ITableStorageService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration, string tableName)
        {
            var connectionString = configuration.GetConnectionString("AzureTableStorage");
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task AddEntityAsync(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        public async Task<T> GetEntityAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAllEntitiesAsync()
        {
            var entities = _tableClient.QueryAsync<T>();
            var results = new List<T>();
            await foreach (var entity in entities)
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task UpdateEntityAsync(T entity)
        {
            await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}

