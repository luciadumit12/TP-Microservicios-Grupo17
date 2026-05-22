//NOTFOUNDEXCEPTION.CS
//se lanza cuando algo no se encuentra
//en Notifications.API se usa en dos casos:
//1. cuando el usuario no existe en Users.API → NTF-001
//2. cuando el usuario no tiene notificaciones → NTF-003
//el NotFoundExceptionHandler la atrapa y devuelve 404
namespace Notifications.API.Exceptions
{
    //recibe dos datos cuando se lanza: el codigo del error y el mensaje
    //por ej: "NTF-001" y "El usuario destinatario no fue encontrado."
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        //guarda el codigo del error para que el Handler lo pueda usar en respuesta JSON
        public string ErrorCode { get; } = errorCode;
    }
}