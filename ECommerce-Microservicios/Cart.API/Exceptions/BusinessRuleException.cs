namespace Cart.API.Exceptions
{
    /// <summary>
    /// Excepción utilizada cuando se viola una regla de negocio.
    /// </summary>
    public class BusinessRuleException(string errorCode, string message)
        : Exception(message)
    {
        /// <summary>
        /// Código de error asociado a la excepción.
        /// </summary>
        public string ErrorCode { get; } = errorCode;
    }
}