namespace Cart.API.Exceptions
{
    /// <summary>
    /// Excepción utilizada cuando un recurso no fue encontrado.
    /// </summary>
    public class NotFoundException(string errorCode, string message)
        : Exception(message)
    {
        /// <summary>
        /// Código de error asociado a la excepción.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}