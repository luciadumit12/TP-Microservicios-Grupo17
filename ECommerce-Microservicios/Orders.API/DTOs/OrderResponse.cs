//ORDERRESPONSE.CS
//lo que la API devuelve en cualquier endpoint exitoso (GET, POST, PUT)
//incluye todo: id, items con precios, total, estado y fecha
//es lo que ve el cliente cuando hace una llamada exitosa
namespace Orders.API.DTOs
{
    /// <summary>
    /// Respuesta que devuelve la API cuando una operacion fue exitosa
    /// </summary>
    /// <example>
    /// {
    ///   "id": "f1e2d3c4-0000-0000-0000-aabbccddeeff",
    ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "items": [
    ///     {
    ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "cantidad": 2,
    ///       "precioUnitario": 1500.00
    ///     }
    ///   ],
    ///   "total": 3000.00,
    ///   "estado": "Pendiente",
    ///   "fechaCreacion": "2024-03-10T11:00:00Z"
    /// }
    /// </example>
    public class OrderResponse
    {
        //id unico de la orden generado por el sistema
        /// <summary>ID unico de la orden generado por el sistema</summary>
        public Guid Id { get; set; }

        //id del usuario que hizo la orden
        /// <summary>ID del usuario que realizo la orden</summary>
        public Guid UsuarioId { get; set; }

        //lista de productos con sus precios
        /// <summary>Lista de productos comprados con sus precios al momento de la compra</summary>
        public List<OrderItemResponse> Items { get; set; } = new();

        //total calculado automaticamente por el sistema
        /// <summary>Total de la orden calculado automaticamente sumando cantidad por precio de cada item</summary>
        public decimal Total { get; set; }

        //estado actual de la orden: Pendiente, Confirmada, Enviada, Entregada o Cancelada
        /// <summary>Estado actual de la orden. Valores posibles: Pendiente, Confirmada, Enviada, Entregada, Cancelada</summary>
        public string Estado { get; set; } = string.Empty;

        //fecha y hora en que se creo la orden
        /// <summary>Fecha y hora en que se creo la orden, asignada automaticamente por el sistema</summary>
        public DateTime FechaCreacion { get; set; }
    }
}