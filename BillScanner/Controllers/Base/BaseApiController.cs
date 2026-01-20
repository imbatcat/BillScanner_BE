using Asp.Versioning;
using BillScanner.Helpers.RequestHelpers;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers.Base
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Returns an <see cref="ActionResult"/> containing paginated data and pagination metadata.
        /// </summary>
        /// <typeparam name="T">The type of items in the paginated list.</typeparam>
        /// <param name="items">The list of items for the current page.</param>
        /// <param name="count">The total number of items available.</param>
        /// <param name="pageIndex">The current page index (one-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing a <see cref="Pagination{T}"/> object.</returns>
        protected static Pagination<T> ResultWithPagination<T>(IReadOnlyList<T> items, int count, int pageIndex, int pageSize)
        {
            var pagination = new Pagination<T>(items, count, pageIndex, pageSize);
            return pagination;
        }

        #region Status Code Helpers with Messages

        /// <summary>
        /// Returns a 400 Bad Request with a message
        /// </summary>
        protected ObjectResult BadRequestWithMessage(string message)
        {
            return BadRequest(new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 401 Unauthorized with a message
        /// </summary>
        protected ObjectResult UnauthorizedWithMessage(string message)
        {
            return Unauthorized(new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 403 Forbidden with a message
        /// </summary>
        protected ObjectResult ForbiddenWithMessage(string message)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 404 Not Found with a message
        /// </summary>
        protected ObjectResult NotFoundWithMessage(string message)
        {
            return NotFound(new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 409 Conflict with a message
        /// </summary>
        protected ObjectResult ConflictWithMessage(string message)
        {
            return Conflict(new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 500 Internal Server Error with a message
        /// </summary>
        protected ObjectResult InternalServerErrorWithMessage(string message)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a custom status code with a message
        /// </summary>
        protected ObjectResult StatusCodeWithMessage(int statusCode, string message)
        {
            return StatusCode(statusCode, new ErrorResponse { Message = message });
        }

        /// <summary>
        /// Returns a 200 OK with a message
        /// </summary>
        protected ObjectResult OkWithMessage(string message)
        {
            return Ok(new SuccessResponse { Message = message });
        }

        /// <summary>
        /// Returns a 201 Created with a message and optional data
        /// </summary>
        protected ObjectResult CreatedWithMessage(string message, object? data = null)
        {
            return StatusCode(StatusCodes.Status201Created, new SuccessResponse { Message = message, Data = data });
        }

        #endregion
    }

    #region Response Models

    /// <summary>
    /// Standard error response format
    /// </summary>
    public record ErrorResponse
    {
        public string Message { get; init; } = null!;
        public string? Details { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }
    }

    /// <summary>
    /// Standard success response format
    /// </summary>
    public record SuccessResponse
    {
        public string Message { get; init; } = null!;
        public object? Data { get; init; }
    }

    #endregion
}