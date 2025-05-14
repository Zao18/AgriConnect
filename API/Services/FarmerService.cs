using API.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    public class FarmerService : IFarmerService
    {
        private readonly TableClient _tableClient;

        public FarmerService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("AzureStorage");
            _tableClient = new TableClient(connectionString, "Farmers");
            _tableClient.CreateIfNotExists();
        }

        public async Task<IEnumerable<FarmerEntity>> GetAllAsync()
        {
            return _tableClient.Query<FarmerEntity>(f => f.PartitionKey == "Farmer").ToList();
        }

        public async Task<FarmerEntity> GetByIdAsync(string id)
        {
            var result = await _tableClient.GetEntityAsync<FarmerEntity>("Farmer", id);
            return result.Value;
        }

        public async Task AddAsync(FarmerEntity farmer)
        {
            farmer.PartitionKey = "Farmer";
            farmer.RowKey = Guid.NewGuid().ToString();
            await _tableClient.AddEntityAsync(farmer);
        }

        public async Task UpdateAsync(string id, FarmerEntity farmer)
        {
            farmer.PartitionKey = "Farmer";
            farmer.RowKey = id;
            await _tableClient.UpdateEntityAsync(farmer, farmer.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteAsync(string id)
        {
            await _tableClient.DeleteEntityAsync("Farmer", id);
        }
    }
}
