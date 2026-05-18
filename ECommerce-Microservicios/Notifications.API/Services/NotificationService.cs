//El Service es el cerebro de Notifications.API
//Toda la logica de negocio vive aca
//El Controller no decide nada, le pasa todo al NotificationService
//A diferencia de Orders.API, este Service se conecta con Users.API
//para verificar si el usuario existe antes de crear la notificacion

//nombres de las carpetas de las clases que se nombran en este archivo
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        //lista en memoria donde se guardan las notificaciones mientras la app esta corriendo
        //cuando la catedra entregue la libreria de persistencia, esta linea se reemplaza
        //por la conexion real a la base de datos
        private readonly List<Notification> _notificaciones = new();

        //lista de los tipos de notificacion validos
        //si el cliente manda un tipo que no esta en esta lista, el sistema lo rechaza
        private readonly List<string> _tiposValidos = new() { "Email", "Push", "SMS" };

        //variable que guarda el HttpClient para poder conectarse con Users.API
        //se registra en Program.cs con AddHttpClient("UsersAPI")
        private readonly IHttpClientFactory _httpClientFactory;

        //cuando el Service arranca, .NET le entrega el HttpClientFactory automaticamente
        //gracias a que lo registramos en Program.cs con AddHttpClient("UsersAPI")
        public NotificationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        //METODO 1: EnviarNotificacion
        //el Controller le pide que cree una notificacion nueva
        //primero valida que los datos esten bien
        //despues se conecta a Users.API para verificar si el usuario existe
        //si el usuario no existe lanza NotFoundException con NTF-001
        //si el usuario existe crea la notificacion y la guarda en la lista
        //el async/await significa que espera la respuesta de Users.API antes de continuar
        public async Task<NotificationResponse> EnviarNotificacion(SendNotificationRequest request)
        {
            //valida que el mensaje no este vacio y que el tipo sea Email, Push o SMS
            //si alguno de estos datos esta mal lanza ValidationException con NTF-002
            if (string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo) ||
                !_tiposValidos.Contains(request.Tipo))
                throw new ValidationException("NTF-002", "Los datos de la notificacion son invalidos.");

            //crea el HttpClient configurado para hablar con Users.API
            var client = _httpClientFactory.CreateClient("UsersAPI");
            //arma la URL para consultar si el usuario existe en Users.API
            //por ej: api/users/a1b2c3d4-...
            var url = $"api/users/{request.UsuarioId}";
            //muestra en la consola la URL que se esta llamando, util para depurar
            Console.WriteLine($"[DEBUG] Llamando a: {client.BaseAddress}{url}");

            //le pregunta a Users.API si el usuario existe
            var response = await client.GetAsync(url);
            //muestra en la consola el resultado de la llamada a Users.API
            Console.WriteLine($"[DEBUG] Status: {response.StatusCode}");

            //si Users.API responde que el usuario no existe, lanza NotFoundException con NTF-001
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            //si el usuario existe, crea la notificacion con todos sus campos
            //el id lo genera el sistema automaticamente
            //el estado arranca en Enviada porque ya se verifico que el usuario existe
            //la fecha la asigna el sistema automaticamente
            var notificacion = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada",
                FechaEnvio = DateTime.UtcNow
            };

            //guarda la notificacion en la lista en memoria
            _notificaciones.Add(notificacion);
            //convierte la notificacion en NotificationResponse y la devuelve al Controller
            return MapearAResponse(notificacion);
        }

        //METODO 2: ObtenerPorUsuario
        //el Controller le pide todas las notificaciones de un usuario especifico
        //busca en la lista todas las notificaciones que tengan ese usuarioId
        //si el usuario no tiene notificaciones lanza NotFoundException con NTF-003
        //si tiene notificaciones las convierte en NotificationResponse y las devuelve
        public List<NotificationResponse> ObtenerPorUsuario(Guid usuarioId)
        {
            //busca todas las notificaciones de ese usuario en la lista
            var notificaciones = _notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .ToList();

            //si no tiene notificaciones avisa con NTF-003
            if (notificaciones.Count == 0)
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

            //convierte cada Notification en NotificationResponse y las devuelve al Controller
            return notificaciones.Select(MapearAResponse).ToList();
        }

        //este metodo convierte una Notification del sistema en un NotificationResponse que ve el cliente
        //lo usan todos los metodos del Service antes de devolver algo al Controller
        //es privado porque solo lo usa el Service internamente
        private static NotificationResponse MapearAResponse(Notification n) => new()
        {
            Id = n.Id,
            UsuarioId = n.UsuarioId,
            Mensaje = n.Mensaje,
            Tipo = n.Tipo,
            Estado = n.Estado,
            FechaEnvio = n.FechaEnvio
        };
    }
}