//NOTFOUNDEXCEPTION.CS
//se lanza cuando algo no se encuentra
//por ej cuando se busca una orden por id y no existe
//el NotFoundExceptionHandler la atrapa y devuelve 404 con el codigo ORD-001
namespace Orders.API.Exceptions
{
    //recibe dos datos cuando se lanza: el codigo del error y el mensaje
    //por ej: "ORD-001" y "Orden no encontrada."
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        //guarda el codigo del error para que el Handler lo pueda usar en la respuesta JSON
        public string ErrorCode { get; } = errorCode;
    }
}