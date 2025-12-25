namespace EduocationSystem.Shared.Responses
{
    /// <summary>
    /// Generic service response wrapper for consistent API responses
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ServiceResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Main response message in English and Arabic
        /// </summary>
        public string Message { get; set; } = string.Empty;
        public string MessageAr { get; set; } = string.Empty;

        /// <summary>
        /// The actual data being returned
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// List of error messages if operation failed
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// HTTP status code for the response
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Timestamp when the response was created
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create a successful response
        /// </summary>
        /// <param name="data">Data to return</param>
        /// <param name="message">Success message</param>
        /// <returns>ServiceResponse with success status</returns>
        public static ServiceResponse<T> SuccessResponse(T? data, string message = "Operation completed successfully", string messageAr = "العملية تمت بنجاح")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = true,
                Message = message,
                MessageAr = messageAr,
                Data = data,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create an error response with single error message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>ServiceResponse with error status</returns>
        public static ServiceResponse<T> ErrorResponse(string message, string messageAr, int statusCode = 400)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create an error response with multiple error messages
        /// </summary>
        /// <param name="errors">List of error messages</param>
        /// <param name="message">Main error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>ServiceResponse with error status</returns>
        public static ServiceResponse<T> ErrorResponse(List<string> errors, string message = "Operation failed", string messageAr = "فشل العملية", int statusCode = 400)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create an error response for validation failures
        /// </summary>
        /// <param name="validationErrors">Dictionary of field validation errors</param>
        /// <param name="message">Main validation message</param>
        /// <returns>ServiceResponse with validation error status</returns>
        public static ServiceResponse<T> ValidationErrorResponse(Dictionary<string, List<string>> validationErrors, string message = "Validation failed", string messageAr = "فشل التحقق من الصحة")
        {
            var errors = new List<string>();
            foreach (var kvp in validationErrors)
            {
                foreach (var error in kvp.Value)
                {
                    errors.Add($"{kvp.Key}: {error}");
                }
            }

            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = errors,
                StatusCode = 400,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a not found error response
        /// </summary>
        /// <param name="message">Not found message</param>
        /// <returns>ServiceResponse with 404 status</returns>
        public static ServiceResponse<T> NotFoundResponse(string message = "Resource not found", string messageAr = "المورد غير موجود")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = 404,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create an unauthorized error response
        /// </summary>
        /// <param name="message">Unauthorized message</param>
        /// <returns>ServiceResponse with 401 status</returns>
        public static ServiceResponse<T> UnauthorizedResponse(string message = "Unauthorized access", string messageAr = "الوصول غير مصرح به")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a forbidden error response
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>ServiceResponse with 403 status</returns>
        public static ServiceResponse<T> ForbiddenResponse(string message = "Access forbidden", string messageAr = "الوصول محظور")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = 403,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create a conflict error response
        /// </summary>
        /// <param name="message">Conflict message</param>
        /// <returns>ServiceResponse with 409 status</returns>
        public static ServiceResponse<T> ConflictResponse(string message = "Resource conflict", string messageAr = "تعارض المورد")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = 409,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create an internal server error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>ServiceResponse with 500 status</returns>
        public static ServiceResponse<T> InternalServerErrorResponse(string message = "Internal server error occurred", string messageAr = "حدث خطأ داخلي في الخادم")
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                MessageAr = messageAr,
                Errors = new List<string> { message },
                StatusCode = 500,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}