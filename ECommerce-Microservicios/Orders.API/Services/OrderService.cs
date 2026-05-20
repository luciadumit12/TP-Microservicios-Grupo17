//El Service es el cerebro de Orders.API
//Toda la logica de negocio vive aca
//cada vez que el Controller recibe una llamada HTTP, le pasa la llamada al OrderService para que la procese
//por ej cuando llega un POST /api/orders, el OrderService valida el usuario, los productos, el stock y crea la orden

//nombres de las carpetas de las clases que se nombran en este archivo
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrderService
    {
        //lista en memoria donde se guardan las ordenes mientras la app esta corriendo
        //con la libreria que nos de el profe vamos a reemplazar esta linea por la conexion real a la base de datos
        private readonly List<Order> _ordenes = new();

        //variable que guarda el HttpClientFactory para poder conectarse con Users.API y Products.API
        //se registra en Program.cs con AddHttpClient("UsersAPI") y AddHttpClient("ProductsAPI")
        private readonly IHttpClientFactory _httpClientFactory;

        //cuando el Service arranca, .NET le entrega el HttpClientFactory automaticamente
        //gracias a que lo registramos en Program.cs con AddHttpClient
        public OrderService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        //define que cambios de estado son validos
        //por ej: si una orden esta en Pendiente, solo puede pasar a Confirmada o Cancelada
        //si esta en Entregada, no puede pasar a ningun lado
        private static readonly Dictionary<string, List<string>> TransicionesValidas = new()
        {
            { "Pendiente",  new() { "Confirmada", "Cancelada" } },
            { "Confirmada", new() { "Enviada", "Cancelada" } },
            { "Enviada",    new() { "Entregada" } },
            { "Entregada",  new() { } },
            { "Cancelada",  new() { } }
        };

        //METODO 1: ObtenerTodas
        //el Controller le pide todas las ordenes
        //si viene un usuarioId filtra por ese usuario
        //si no viene ningun usuarioId devuelve todas las ordenes
        //convierte cada Order en un OrderResponse antes de devolvérselo al Controller
        public List<OrderResponse> ObtenerTodas(Guid? usuarioId)
        {
            var ordenes = _ordenes.AsEnumerable();

            //si el cliente mando un usuarioId, filtra solo las ordenes de ese usuario
            if (usuarioId.HasValue)
                ordenes = ordenes.Where(o => o.UsuarioId == usuarioId.Value);

            //convierte cada Order en OrderResponse y los devuelve en una lista
            return ordenes.Select(MapearAResponse).ToList();
        }

        //METODO 2: ObtenerPorId
        //el Controller le pide una orden especifica por su id
        //si la orden no existe lanza NotFoundException con el codigo ORD-001
        //el NotFoundExceptionHandler la atrapa y devuelve 404
        //si la orden existe la convierte en OrderResponse y la devuelve
        public OrderResponse ObtenerPorId(Guid id)
        {
            var orden = _ordenes.FirstOrDefault(o => o.Id == id)
                ?? throw new NotFoundException("ORD-001", "Orden no encontrada.");

            return MapearAResponse(orden);
        }

        //METODO 3: CrearOrden
        //el Controller le pide que cree una orden nueva
        //el metodo es async porque necesita esperar las respuestas de Users.API y Products.API
        //primero valida que la orden tenga al menos un item
        //despues verifica que el usuario exista en Users.API → ORD-003
        //despues verifica que cada producto exista en Products.API → ORD-004
        //despues verifica que haya stock suficiente para cada producto → ORD-005
        //si todo esta bien crea la orden con los precios reales y la guarda en la lista
        public async Task<OrderResponse> CrearOrden(CreateOrderRequest request)
        {
            //si el cliente mando una orden sin items, avisa que los datos son invalidos
            if (request.Items == null || request.Items.Count == 0)
                throw new ValidationException("ORD-002", "Los datos de la orden son invalidos.");

            //ORD-003: verifica que el usuario exista en Users.API
            //crea el HttpClient configurado para hablar con Users.API
            var usersClient = _httpClientFactory.CreateClient("UsersAPI");
            //le pregunta a Users.API si el usuario existe, no necesita DTO porque solo le importa el status code de la respuesta
            var userResponse = await usersClient.GetAsync($"api/users/{request.UsuarioId}");
            //si Users.API responde que no existe, lanza NotFoundException con ORD-003
            if (!userResponse.IsSuccessStatusCode)
                throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");

            //ORD-004 y ORD-005: verifica que cada producto exista y tenga stock suficiente en Products.API
            var productsClient = _httpClientFactory.CreateClient("ProductsAPI");
            var items = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                //le pregunta a Products.API si el producto existe
                var productResponse = await productsClient.GetAsync($"api/products/{item.ProductoId}");
                //si Products.API responde que no existe, lanza NotFoundException con ORD-004
                if (!productResponse.IsSuccessStatusCode)
                    throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

                //deserializa la respuesta de Products.API para obtener el precio y el stock
                //ProductoDto es una clase interna que solo usa el OrderService para leer la respuesta de Products.API
                //no va en la carpeta DTOs porque no es un objeto que ve el cliente
                var product = await productResponse.Content.ReadFromJsonAsync<ProductoDto>()
                    ?? throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

                //ORD-005: verifica que haya stock suficiente para la cantidad solicitada
                if (product.Stock < item.Cantidad)
                    throw new BusinessRuleException("ORD-005",
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.");

                //si el producto existe y hay stock, agrega el item con el precio real de Products.API
                items.Add(new OrderItem
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = product.Precio
                });
            }

            //crea la orden con todos sus campos
            //el id lo genera el sistema automaticamente
            //el total se calcula sumando cantidad por precio real de cada item
            //el estado arranca siempre en Pendiente
            //la fecha la asigna el sistema automaticamente
            var orden = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = items,
                Total = items.Sum(i => i.Cantidad * i.PrecioUnitario),
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            //guarda la orden en la lista en memoria
            _ordenes.Add(orden);

            //convierte la orden en OrderResponse y la devuelve al Controller
            return MapearAResponse(orden);
        }

        //METODO 4: ActualizarEstado
        //el Controller le pide que cambie el estado de una orden
        //primero busca la orden por id, si no existe lanza NotFoundException con ORD-001
        //despues verifica si el cambio de estado es valido segun TransicionesValidas
        //si no es valido lanza BusinessRuleException con ORD-006
        //si es valido cambia el estado y devuelve la orden actualizada
        public OrderResponse ActualizarEstado(Guid id, UpdateOrderStatusRequest request)
        {
            //busca la orden, si no existe avisa con ORD-001
            var orden = _ordenes.FirstOrDefault(o => o.Id == id)
                ?? throw new NotFoundException("ORD-001", "Orden no encontrada.");

            //verifica si el cambio de estado es valido
            //por ej no puede pasar de Entregada a Pendiente
            if (!TransicionesValidas[orden.Estado].Contains(request.Estado))
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{orden.Estado}' no puede cambiar a '{request.Estado}'.");

            //si el cambio es valido, actualiza el estado
            orden.Estado = request.Estado;

            //convierte la orden actualizada en OrderResponse y la devuelve al Controller
            return MapearAResponse(orden);
        }

        //este metodo convierte un Order del sistema en un OrderResponse que ve el cliente
        //lo usan todos los metodos del Service antes de devolver algo al Controller
        //es privado porque solo lo usa el Service internamente
        private static OrderResponse MapearAResponse(Order orden) => new()
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Items = orden.Items.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = orden.Total,
            Estado = orden.Estado,
            FechaCreacion = orden.FechaCreacion
        };

        //DTO INTERNO: Para leer el mensaje http entre APIS, no entre apli-cliente.
        //ProductoDto representa los datos que devuelve Products.API cuando se consulta un producto
        //solo lo usa el OrderService internamente para leer el precio y el stock
        //no va en la carpeta DTOs porque no es un objeto que ve el cliente
        private class ProductoDto
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public int Stock { get; set; }
        }
    }
}