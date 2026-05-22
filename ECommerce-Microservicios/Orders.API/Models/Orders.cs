//.Los Models es la representacion de como existen los datos dentro del sistema
//Son las entidades reales que se guardan en la base de datos
//Son distintos a los DTOs porque los DTOs son lo que ve el cliente
//y los Models son lo que guarda el sistema internamente
//El modelado de datos.

//ORDER.CS
//Cuando OrderService crea una orden, este model representa la orden de compra que se crea dentro del sistema, crea un objeto basado en esta clase
namespace Orders.API.Models
{
    public class Order
    {
        //id unico de la orden, lo genera el sistema automaticamente
        public Guid Id { get; set; }
        //id del usuario que hizo la orden
        public Guid UsuarioId { get; set; }
        //lista de productos incluidos en la orden
        //empieza vacia, el sistema la llena con los items cuando crea la orden
        public List<OrderItem> Items { get; set; } = new();
        //total de la orden, calculado automaticamente sumando cantidad por precio de cada item
        public decimal Total { get; set; }
        //estado actual de la orden
        //arranca siempre en Pendiente cuando se crea una orden nueva
        //puede cambiar a: Confirmada, Enviada, Entregada o Cancelada
        public string Estado { get; set; } = "Pendiente";
        //fecha y hora en que se creo la orden, la asigna el sistema automaticamente
        public DateTime FechaCreacion { get; set; }
    }
}