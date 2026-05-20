namespace Cart.API.Exceptions
{
    /// <summary>
    /// Excepción utilizada cuando los datos enviados son inválidos.
    /// </summary>
    public class ValidationException(string errorCode, string message)
        : Exception(message)
    {
        /// <summary>
        /// Código de error asociado a la excepción.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}