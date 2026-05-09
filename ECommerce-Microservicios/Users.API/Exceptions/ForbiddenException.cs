// Se lanza cuando el usuario está bloqueado → HTTP 403
namespace Users.API.Exceptions
{
    public class ForbiddenException : Exception
    {
        public string ErrorCode { get; }
        public ForbiddenException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}