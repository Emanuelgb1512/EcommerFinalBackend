namespace EcommerceAPI.Models
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public AppException(string mensaje, int statusCode = 400) : base(mensaje)
        {
            StatusCode = statusCode;
        }
    }
}