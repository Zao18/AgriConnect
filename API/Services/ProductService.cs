using API.Models;

namespace API.Services
{
    public class ProductService : IProductService
    {
        private readonly List<ProductModel> _products = new();

        public Task<IEnumerable<ProductModel>> GetAllAsync() => Task.FromResult(_products.AsEnumerable());

        public Task<ProductModel> GetByIdAsync(string id) => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

        public Task AddAsync(ProductModel product)
        {
            _products.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(string id, ProductModel updated)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is not null)
            {
                product.ProductName = updated.ProductName;
                product.Description = updated.Description;
                product.StockLevel = updated.StockLevel;
                product.Price = updated.Price;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null) _products.Remove(product);
            return Task.CompletedTask;
        }
    }
}
