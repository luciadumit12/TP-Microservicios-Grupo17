// Se lanza cuando las credenciales son incorrectas → HTTP 401
namespace Users.API.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public string ErrorCode { get; }
        public UnauthorizedException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}