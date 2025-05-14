using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductEntity>> GetAllAsync();
        Task<ProductEntity> GetByIdAsync(string id);
        Task AddAsync(ProductEntity product);
        Task UpdateAsync(string id, ProductEntity product);
        Task DeleteAsync(string id);
    }
}

