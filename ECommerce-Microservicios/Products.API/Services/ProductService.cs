// El Service es el cerebro de Products.API
// Toda la logica de negocio vive aca
// cada vez que el Controller recibe una llamada HTTP, le pasa la llamada al ProductService para que la procese
using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;
using System.Net.Http.Json;

namespace Products.API.Services
{
    public class ProductService
    {
        // variable que guarda el Repository para poder hablar con la base de datos SQLite
        // reemplaza la lista en memoria que teniamos antes
        private readonly ProductRepository _repository;

        // variable que guarda el HttpClient para poder conectarse con Orders.API
        // se usa en el DeleteAsync para verificar si el producto tiene ordenes activas (PRD-004)
        private readonly HttpClient _ordersClient;

        // cuando el Service arranca, .NET le entrega el Repository y el HttpClientFactory automaticamente
        // gracias a que los registramos en Program.cs
        public ProductService(ProductRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _ordersClient = httpClientFactory.CreateClient("OrdersAPI");
        }

        // GET /api/products
        // le pide al Repository todos los productos filtrados por categoria y/o nombre
        public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
        {
            var products = await _repository.ObtenerTodos(categoria, nombre);
            return products.Select(MapToResponse);
        }

        // GET /api/products/{id}
        // le pide al Repository el producto por su id
        // si no existe lanza PRD-001 → NotFoundExceptionHandler devuelve 404
        public async Task<ProductResponse> GetByIdAsync(Guid id)
        {
            var product = await _repository.ObtenerPorId(id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return MapToResponse(product);
        }

        // POST /api/products
        // valida los datos, verifica duplicados y guarda el producto en la base de datos
        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            // validacion de campos → PRD-002
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                request.Precio <= 0 ||
                request.Stock < 0 ||
                string.IsNullOrWhiteSpace(request.Categoria))
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            // validacion de duplicado → PRD-003
            var existe = await _repository.ExistePorNombreYCategoria(request.Nombre, request.Categoria);
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

            // le pide al Repository que guarde el producto en la base de datos
            await _repository.Guardar(product);
            return MapToResponse(product);
        }

        // PUT /api/products/{id}
        // busca el producto, valida los datos y actualiza en la base de datos
        public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _repository.ObtenerPorId(id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            // validacion de campos → PRD-002
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                request.Precio <= 0 ||
                request.Stock < 0 ||
                string.IsNullOrWhiteSpace(request.Categoria))
                throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

            product.Nombre = request.Nombre;
            product.Descripcion = request.Descripcion;
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria;

            // le pide al Repository que actualice el producto en la base de datos
            await _repository.Actualizar(product);
            return MapToResponse(product);
        }

        // DELETE /api/products/{id}
        // verifica si tiene ordenes activas en Orders.API → PRD-004
        // si no tiene ordenes activas elimina el producto de la base de datos
        public async Task DeleteAsync(Guid id)
        {
            var product = await _repository.ObtenerPorId(id)
                ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");

            // PRD-004: consulta a Orders.API si el producto tiene ordenes activas
            var response = await _ordersClient.GetAsync("api/orders");
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
                var tieneOrdenes = orders?.Any(o =>
                    o.Items.Any(i => i.ProductoId == id) &&
                    (o.Estado == "Pendiente" || o.Estado == "Confirmada"));

                if (tieneOrdenes == true)
                    throw new BusinessRuleException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");
            }

            // le pide al Repository que elimine el producto de la base de datos
            await _repository.Eliminar(id);
        }

        // convierte Product (modelo interno) a ProductResponse (lo que ve el cliente)
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

    // DTO INTERNO: para leer la respuesta de Orders.API
    // solo lo usa ProductService internamente para verificar ordenes activas
    // no va en la carpeta DTOs porque no es un objeto que ve el cliente
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid ProductoId { get; set; }
    }
}