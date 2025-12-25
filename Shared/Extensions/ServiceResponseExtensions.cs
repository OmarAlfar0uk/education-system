using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Shared.Extensions
{
    /// <summary>
    /// Extension methods for ServiceResponse to easily convert to ActionResult
    /// </summary>
    public static class ServiceResponseExtensions
    {
        /// <summary>
        /// Convert ServiceResponse to appropriate ActionResult based on StatusCode
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <returns>ActionResult with appropriate HTTP status code</returns>
        public static ActionResult<ServiceResponse<T>> ToActionResult<T>(this ServiceResponse<T> response)
        {
            return response.StatusCode switch
            {
                200 => new OkObjectResult(response),
                201 => new ObjectResult(response) { StatusCode = 201 },
                400 => new BadRequestObjectResult(response),
                401 => new UnauthorizedObjectResult(response),
                403 => new ObjectResult(response) { StatusCode = 403 },
                404 => new NotFoundObjectResult(response),
                409 => new ConflictObjectResult(response),
                500 => new ObjectResult(response) { StatusCode = 500 },
                _ => new ObjectResult(response) { StatusCode = response.StatusCode }
            };
        }

        /// <summary>
        /// Convert ServiceResponse to ActionResult with CreatedAtAction for POST operations
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <param name="actionName">Action name for CreatedAtAction</param>
        /// <param name="routeValues">Route values for CreatedAtAction</param>
        /// <returns>CreatedAtActionResult if success, otherwise appropriate error result</returns>
        public static ActionResult<ServiceResponse<T>> ToCreatedActionResult<T>(
            this ServiceResponse<T> response,
            string actionName,
            object? routeValues = null)
        {
            if (response.IsSuccess && response.StatusCode == 200)
            {
                // Change status code to 201 for created resources
                response.StatusCode = 201;
                return new CreatedAtActionResult(actionName, null, routeValues, response);
            }

            return response.ToActionResult();
        }

        /// <summary>
        /// Extract validation errors from ModelState and convert to ServiceResponse
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="modelState">ModelState from controller</param>
        /// <param name="message">Custom validation message</param>
        /// <returns>ServiceResponse with validation errors</returns>
        public static ServiceResponse<T> ToValidationErrorResponse<T>(
            this Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState,
            string message = "Validation failed")
        {
            var validationErrors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToList() ?? new List<string>()
                );

            return ServiceResponse<T>.ValidationErrorResponse(validationErrors, message);
        }

        /// <summary>
        /// Check if response is successful
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool IsSuccessful<T>(this ServiceResponse<T> response)
        {
            return response.IsSuccess && response.StatusCode >= 200 && response.StatusCode < 300;
        }

        /// <summary>
        /// Get error messages as a single concatenated string
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <param name="separator">Separator for joining messages</param>
        /// <returns>Concatenated error messages</returns>
        public static string GetErrorsAsString<T>(this ServiceResponse<T> response, string separator = "; ")
        {
            return string.Join(separator, response.Errors);
        }

        /// <summary>
        /// Add additional error to existing ServiceResponse
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <param name="error">Error message to add</param>
        /// <returns>Modified ServiceResponse</returns>
        public static ServiceResponse<T> AddError<T>(this ServiceResponse<T> response, string error)
        {
            response.Errors.Add(error);
            response.IsSuccess = false;
            return response;
        }

        /// <summary>
        /// Add multiple errors to existing ServiceResponse
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="response">ServiceResponse instance</param>
        /// <param name="errors">Error messages to add</param>
        /// <returns>Modified ServiceResponse</returns>
        public static ServiceResponse<T> AddErrors<T>(this ServiceResponse<T> response, IEnumerable<string> errors)
        {
            response.Errors.AddRange(errors);
            response.IsSuccess = false;
            return response;
        }
    }
}