using BillScanner.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Business.Handlers.Authentication.Login.Dto;
using Business.Handlers.Authentication.Register.Dto;
using BillScanner.Models;
using Business.Handlers.Authentication.Logout.Dto;
using System.ComponentModel.DataAnnotations;

namespace BillScanner.Controllers
{
    public class AuthController(IMediator mediator) : BaseApiController
    {
        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequestWithMessage("Email and password are required");
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(request.Email))
            {
                return BadRequestWithMessage("Invalid email address");
            }

            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.DisplayName
            };

            var response = await mediator.Send(command);

            return CreatedAtAction(nameof(Register), response);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequestWithMessage("Email and password are required");
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(request.Email))
            {
                return BadRequestWithMessage("Invalid email address");
            }

            var query = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await mediator.Send(query);

            return Ok(response);
        }

        /// <summary>
        /// Logout current user and revoke refresh token
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return UnauthorizedWithMessage("User not authenticated");
            }

            var command = new LogoutCommand { UserId = userIdClaim };

            await mediator.Send(command);

            return OkWithMessage("Logged out successfully");
        }
    }
}