// El Service contiene toda la lógica de negocio de Notifications.API.
// El Controller no piensa — solo recibe el request y llama al Service.
// A diferencia de Users.API, este Service se conecta con Users.API
// para verificar si el usuario existe antes de crear la notificación
// Ahora usa SQLite como base de datos a través del NotificationRepository

using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services
{
    public class NotificationService
    {
        //variable que guarda el Repository para poder hablar con la base de datos SQLite
        //reemplaza la lista en memoria que teniamos antes
        private readonly NotificationRepository _repository;

        // Tipos de notificación válidos según el TP
        // Si el cliente manda un tipo que no está en esta lista, se rechaza con NTF-002
        private readonly List<string> _tiposValidos = new() { "Email", "Push", "SMS" };

        // HttpClientFactory para conectarse con Users.API
        // Se registra en Program.cs con AddHttpClient("UsersAPI")
        private readonly IHttpClientFactory _httpClientFactory;

        // .NET inyecta el Repository y el HttpClientFactory automaticamente
        // gracias a que los registramos en Program.cs
        public NotificationService(NotificationRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        // ─────────────────────────────
        // ENVIAR NOTIFICACIÓN
        // POST /api/notifications/send → 201 Created
        // el metodo es async porque espera la respuesta de Users.API y de la base de datos
        // ─────────────────────────────
        public async Task<NotificationResponse> EnviarNotificacion(SendNotificationRequest request)
        {
            // NTF-002: validar que el mensaje no esté vacío y que el tipo sea válido → 400 Bad Request
            if (string.IsNullOrWhiteSpace(request.Mensaje) ||
                string.IsNullOrWhiteSpace(request.Tipo) ||
                !_tiposValidos.Contains(request.Tipo))
                throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");

            // NTF-001: verificar que el usuario existe en Users.API → 404 Not Found
            // Creamos el HttpClient configurado para hablar con Users.API
            var client = _httpClientFactory.CreateClient("UsersAPI");
            var response = await client.GetAsync($"api/users/{request.UsuarioId}");

            // Si Users.API responde que el usuario no existe → NTF-001
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            // Leemos la respuesta de Users.API para verificar que el usuario está activo
            // UserDto es un DTO interno que solo usa este Service para leer la respuesta de Users.API
            // no va en la carpeta DTOs porque el cliente nunca lo ve
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user is null || !user.Activo)
                throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");

            // Crear la notificación con todos sus campos
            // el id lo genera el sistema automaticamente
            // el estado arranca en Enviada porque el usuario existe
            // la fecha la asigna el sistema automaticamente
            var notificacion = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada",
                FechaEnvio = DateTime.UtcNow
            };

            //le pide al Repository que guarde la notificacion en la base de datos SQLite
            await _repository.Guardar(notificacion);
            return MapearAResponse(notificacion);
        }

        // ─────────────────────────────
        // OBTENER NOTIFICACIONES POR USUARIO
        // GET /api/notifications/{userId} → 200 OK
        // el metodo es async porque espera que el Repository busque en la base de datos
        // ─────────────────────────────
        public async Task<List<NotificationResponse>> ObtenerPorUsuario(Guid usuarioId)
        {
            //le pide al Repository todas las notificaciones de ese usuario en la base de datos
            var notificaciones = (await _repository.ObtenerPorUsuario(usuarioId)).ToList();

            // NTF-003: si el usuario no tiene notificaciones → 404 Not Found
            if (notificaciones.Count == 0)
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

            return notificaciones.Select(MapearAResponse).ToList();
        }

        // ─────────────────────────────
        // MÉTODO PRIVADO: convertir Notification → NotificationResponse
        // Se usa en todos los métodos para no repetir código
        // es privado porque solo lo usa el Service internamente
        // ─────────────────────────────
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

    // ─────────────────────────────
    // DTO INTERNO — solo para uso de NotificationService
    // Lee la respuesta de Users.API cuando consulta si el usuario existe
    // No va en la carpeta DTOs/ porque el cliente nunca lo ve
    // ─────────────────────────────
    public class UserDto
    {
        public Guid Id { get; set; }
        public bool Activo { get; set; }
    }
}