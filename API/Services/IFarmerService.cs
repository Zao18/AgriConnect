using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IFarmerService
    {
        Task<IEnumerable<FarmerEntity>> GetAllAsync();
        Task<FarmerEntity> GetByIdAsync(string id);
        Task AddAsync(FarmerEntity farmer);
        Task UpdateAsync(string id, FarmerEntity farmer);
        Task DeleteAsync(string id);
    }
}
