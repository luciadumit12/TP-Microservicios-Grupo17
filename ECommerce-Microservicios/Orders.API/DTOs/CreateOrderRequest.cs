//DTO: FORMA DE COMUNICARSE CON ORDERS.API, los datos que contienen las llamadas HTTP
//Los DTOs son los objetos que viajan entre el cliente y el Controller
//Lo que el cliente manda no es lo mismo que lo que el sistema guarda internamente

//CREATEORDERREQUEST.CS
//Cliente manda POST para crear una orden
//solo incluye lo que el cliente puede informar: quien compra y que items quiere
//no incluye Id, Total, Estado ni FechaCreacion porque esos los genera el sistema
namespace Orders.API.DTOs
{
    /// <summary>
    /// Datos que manda el cliente para crear una nueva orden
    /// </summary>
    /// <example>
    /// {
    ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "items": [
    ///     {
    ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "cantidad": 2
    ///     }
    ///   ]
    /// }
    /// </example>
    public class CreateOrderRequest
    {
        //id del usuario que esta comprando
        /// <summary>ID del usuario que realiza la compra</summary>
        public Guid UsuarioId { get; set; }

        //lista de productos que quiere comprar
        //empieza vacia, el cliente la llena con los items que quiere
        /// <summary>Lista de productos que quiere comprar con su cantidad</summary>
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}