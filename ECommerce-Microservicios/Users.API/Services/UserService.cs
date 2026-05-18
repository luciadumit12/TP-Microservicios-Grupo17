//El Service es el cerebro de Users.API
//Toda la logica de negocio vive aca
//El Controller no decide nada, le pasa todo al UserService
//El UserService tiene 3 metodos: Register, Login y GetById

//nombres de las carpetas de las clases que se nombran en este archivo
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        //lista en memoria donde se guardan los usuarios mientras la app esta corriendo
        //cuando la catedra entregue la libreria de persistencia, esta linea se reemplaza
        //por la conexion real a la base de datos
        private readonly List<User> _users = [];

        //METODO 1: Register
        //el Controller le pide que registre un usuario nuevo
        //primero valida que todos los campos obligatorios esten completos
        //despues verifica que el email no este registrado ya
        //si todo esta bien crea el usuario y lo guarda en la lista
        public UserResponse Register(RegisterUserRequest request)
        {
            //USR-002: valida que ninguno de los campos obligatorios este vacio
            //si alguno falta lanza ValidationException con USR-002 → devuelve 400
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            //USR-001: verifica que no exista otro usuario con el mismo email
            //si ya existe lanza BusinessRuleException con USR-001 → devuelve 409
            if (_users.Any(u => u.Email == request.Email))
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

            //si paso las dos validaciones, crea el usuario con todos sus campos
            //el id lo genera el sistema automaticamente
            //el usuario arranca con Activo en true e IntentosFallidos en 0
            //la fecha la asigna el sistema automaticamente
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = request.Password,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0
            };

            //guarda el usuario en la lista en memoria
            _users.Add(user);
            //convierte el User en UserResponse y lo devuelve al Controller
            //nunca expone el PasswordHash en la respuesta
            return ToResponse(user);
        }

        //METODO 2: Login
        //el Controller le pide que autentique un usuario
        //primero valida que los campos esten completos
        //despues busca el usuario por email
        //verifica si esta bloqueado y si la contrasena es correcta
        //si falla incrementa los intentos fallidos y bloquea al usuario si llega a 3
        public UserResponse Login(LoginUserRequest request)
        {
            //USR-002: valida que el email y la contrasena no esten vacios
            //si alguno falta lanza ValidationException con USR-002 → devuelve 400
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

            //busca el usuario en la lista por su email
            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            //USR-003: si no existe ningun usuario con ese email
            //lanza UnauthorizedException con USR-003 → devuelve 401
            if (user is null)
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

            //USR-004: si el usuario esta bloqueado por haber superado 3 intentos fallidos
            //lanza ForbiddenException con USR-004 → devuelve 403
            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            //USR-005: si el usuario esta bloqueado por razones de seguridad
            //lanza ForbiddenException con USR-005 → devuelve 403
            if (!user.Activo && user.IntentosFallidos < 3)
                throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            //verifica si la contrasena es incorrecta
            if (user.PasswordHash != request.Password)
            {
                //incrementa el contador de intentos fallidos
                user.IntentosFallidos++;

                //si llego a 3 intentos fallidos, bloquea el usuario y avisa con USR-004
                if (user.IntentosFallidos >= 3)
                {
                    user.Activo = false;
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                //si todavia no llego a 3, avisa que las credenciales son incorrectas con USR-003
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            //si la contrasena es correcta, resetea el contador de intentos fallidos
            user.IntentosFallidos = 0;
            //convierte el User en UserResponse y lo devuelve al Controller
            return ToResponse(user);
        }

        //METODO 3: GetById
        //este metodo lo usa Notifications.API cuando necesita verificar si un usuario existe
        //antes de crear una notificacion, Notifications.API le pregunta a Users.API
        //si el usuario existe llamando a GET /api/users/{id}
        //ese endpoint llama a este metodo
        //si el usuario no existe lanza NotFoundException con USR-003 → devuelve 404
        public UserResponse GetById(Guid id)
        {
            //busca el usuario en la lista por su id
            var user = _users.FirstOrDefault(u => u.Id == id);
            //si no existe avisa con USR-003
            if (user is null)
                throw new NotFoundException("USR-003", "Usuario no encontrado.");
            //si existe lo convierte en UserResponse y lo devuelve
            return ToResponse(user);
        }

        //este metodo convierte un User del sistema en un UserResponse que ve el cliente
        //lo usan todos los metodos del Service antes de devolver algo al Controller
        //es privado porque solo lo usa el Service internamente
        //nunca incluye el PasswordHash en la respuesta para proteger la contrasena
        private static UserResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }
}