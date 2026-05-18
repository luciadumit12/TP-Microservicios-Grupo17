//UPDATEORDERSTATUSREQUEST.CS
//el que administra el sistema manda un PUT con el estado correspondiente para cambiar el estado de una orden
//solo tiene un campo porque lo unico que el administrador puede cambiar es el estado
namespace Orders.API.DTOs
{
    public class UpdateOrderStatusRequest
    {
        //el nuevo estado al que quiere cambiar la orden
        //por ej: Confirmada, Enviada, Entregada o Cancelada
        public string Estado { get; set; } = string.Empty;
    }
}
