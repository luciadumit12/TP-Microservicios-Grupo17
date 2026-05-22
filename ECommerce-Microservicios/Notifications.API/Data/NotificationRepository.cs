//NotificationRepository es el encargado de todas las operaciones con la base de datos
//el NotificationService le pide que guarde o busque notificaciones
//y el Repository se encarga de hablar con SQLite.
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data
{
    public class NotificationRepository
    {
        //guarda la cadena de conexion para poder conectarse a la base de datos
        private readonly string _connectionString;

        //cuando NotificationRepository arranca, recibe la configuracion de la app automaticamente
        //y busca la cadena de conexion en appsettings.json
        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";
        }

        //crea una conexion nueva a la base de datos
        //se usa en cada metodo para abrir y cerrar la conexion automaticamente
        private SqliteConnection CreateConnection() => new(_connectionString);

        //METODO 1: Guardar
        //guarda una notificacion nueva en la base de datos
        public async Task Guardar(Notification notificacion)
        {
            using var conn = CreateConnection();

            //guarda la notificacion en la tabla notifications
            await conn.ExecuteAsync("""
                INSERT INTO notifications (id, usuarioId, mensaje, tipo, estado, fechaEnvio)
                VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
            """, new
            {
                Id = notificacion.Id.ToString(),
                UsuarioId = notificacion.UsuarioId.ToString(),
                notificacion.Mensaje,
                notificacion.Tipo,
                notificacion.Estado,
                FechaEnvio = notificacion.FechaEnvio.ToString("o")
            });
        }

        //METODO 2: ObtenerPorUsuario
        //busca todas las notificaciones de un usuario especifico en la base de datos
        //si no tiene notificaciones devuelve una lista vacia
        public async Task<IEnumerable<Notification>> ObtenerPorUsuario(Guid usuarioId)
        {
            using var conn = CreateConnection();

            //busca todas las notificaciones de ese usuario en la tabla notifications
            return await conn.QueryAsync<Notification>(
                "SELECT * FROM notifications WHERE usuarioId = @UsuarioId",
                new { UsuarioId = usuarioId.ToString() });
        }
    }
}