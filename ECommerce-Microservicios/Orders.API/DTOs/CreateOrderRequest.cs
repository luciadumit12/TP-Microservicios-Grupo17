//DTO: FORMA DE COMUNICARSE CON ORDERS.API, los datos que contienen las llamadas HTTP 
//Los DTOs son los objetos que viajan entre el cliente y el Controller
//Lo que el cliente manda no es lo mismo que lo que el sistema guarda internamente

//CREATEORDERREQUEST.CS
//Cliente manda POST para crear una orden
//solo incluye lo que el cliente puede informar: quien compra y que items quiere
//no incluye Id, Total, Estado ni FechaCreacion porque esos los genera el sistema
namespace Orders.API.DTOs
{
    public class CreateOrderRequest
    {
        //id del usuario que esta comprando
        public Guid UsuarioId { get; set; }
        //lista de productos que quiere comprar
        //empieza vacia, el cliente la llena con los items que quiere
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}