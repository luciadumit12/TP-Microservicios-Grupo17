using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    public class ProductService
    {
        private static readonly List<Product> _products = new();

        public Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
        {
            var query = _products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(query.Select(MapToResponse));
        }

        public Task<ProductResponse> GetByIdAsync(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return Task.FromResult(MapToResponse(product));
        }

        public Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0 || string.IsNullOrWhiteSpace(request.Categoria))
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            var existe = _products.Any(p =>
                p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
                p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));

            if (existe)
                throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria,
                FechaCreacion = DateTime.UtcNow
            };

            _products.Add(product);
            return Task.FromResult(MapToResponse(product));
        }

        public Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = _products.FirstOrDefault(p => p.Id == id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0 || string.IsNullOrWhiteSpace(request.Categoria))
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            product.Nombre = request.Nombre;
            product.Descripcion = request.Descripcion;
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria;

            return Task.FromResult(MapToResponse(product));
        }

        public Task DeleteAsync(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            _products.Remove(product);
            return Task.CompletedTask;
        }

        private static ProductResponse MapToResponse(Product p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            Categoria = p.Categoria,
            FechaCreacion = p.FechaCreacion
        };
    }
}