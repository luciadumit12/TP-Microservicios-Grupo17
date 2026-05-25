//NotificationRepository es el encargado de todas las operaciones con la base de datos
//el NotificationService le pide que guarde o busque notificaciones
//y el Repository se encarga de hablar con SQLite
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data
{
    public class NotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";
            //le dice a Dapper como convertir el texto de SQLite a DateTime
            //es necesario porque SQLite guarda fechas como texto
            SqlMapper.AddTypeHandler(new DateTimeHandler());
            //le dice a Dapper como convertir el texto de SQLite a Guid
            //es necesario porque SQLite guarda los Guid como texto
            SqlMapper.AddTypeHandler(new GuidHandler());
        }

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

            //usa alias para que Dapper pueda mapear correctamente los nombres de columna
            return await conn.QueryAsync<Notification>("""
                SELECT 
                    id AS Id,
                    usuarioId AS UsuarioId,
                    mensaje AS Mensaje,
                    tipo AS Tipo,
                    estado AS Estado,
                    fechaEnvio AS FechaEnvio
                FROM notifications 
                WHERE usuarioId = @UsuarioId
            """, new { UsuarioId = usuarioId.ToString() });
        }
    }

    //convierte el texto guardado en SQLite al tipo DateTime que usa C#
    //es necesario porque SQLite guarda fechas como texto
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(System.Data.IDbDataParameter parameter, DateTime value)
            => parameter.Value = value.ToString("o");

        public override DateTime Parse(object value)
            => DateTime.Parse(value.ToString()!);
    }

    //convierte el texto guardado en SQLite al tipo Guid que usa C#
    //es necesario porque SQLite guarda los Guid como texto
    public class GuidHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
            => parameter.Value = value.ToString();

        public override Guid Parse(object value)
            => Guid.Parse(value.ToString()!);
    }
}