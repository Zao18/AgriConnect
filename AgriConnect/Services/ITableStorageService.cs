using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace AgriConnect.Services
{
    public interface ITableStorageService<T> where T : class, ITableEntity, new()
    {
        Task AddEntityAsync(T entity);
        Task<T> GetEntityAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> GetAllEntitiesAsync();
        Task UpdateEntityAsync(T entity);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
    }
}

