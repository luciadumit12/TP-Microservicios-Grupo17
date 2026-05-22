//.UPDATEORDERSTATUSREQUEST.CS
//el que administra el sistema manda un PUT con el estado correspondiente para cambiar el estado de una orden
//solo tiene un campo porque lo unico que el administrador puede cambiar es el estado
namespace Orders.API.DTOs
{
    /// <summary>
    /// Datos que manda el cliente para cambiar el estado de una orden
    /// </summary>
    /// <example>
    /// {
    ///   "estado": "Confirmada"
    /// }
    /// </example>
    public class UpdateOrderStatusRequest
    {
        //el nuevo estado al que quiere cambiar la orden
        //por ej: Confirmada, Enviada, Entregada o Cancelada
        /// <summary>Nuevo estado de la orden. Valores validos: Confirmada, Enviada, Entregada, Cancelada</summary>
        public string Estado { get; set; } = string.Empty;
    }
}