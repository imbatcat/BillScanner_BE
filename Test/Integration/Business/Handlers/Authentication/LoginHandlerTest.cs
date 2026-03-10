using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BillScanner.Controllers.Base;
using Business.Handlers.Authentication.Login.Dto;
using Domain.Entities;
using FluentAssertions;
using Test.Configuration;
using Xunit.Abstractions;

namespace Test.Integration.Business.Handlers.Authentication
{
    [Collection("BillScannerTestCollection")]
    public class LoginHandlerTest(
        CustomWebApplicationFactory factory,
        ITestOutputHelper outputHelper)
        : BaseTest(factory, outputHelper)
    {
        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            await Factory.ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                DisplayName = "test",
                Email = "test@test.com",
                Password = HashPassword("test"),
                Role = UserRole.User
            };

            await Factory.ExecuteDbContextAsync(async db =>
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            });


            // Act
            var response = await Client.PostAsync(
                "/api/v1/auth/login",
                new StringContent(
                    JsonSerializer.Serialize(new { email = "test@test.com", password = "test" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonSerializerOptions);

            result.Should().NotBeNull();
            result.User.Id.Should().Be(userId);
            result.User.Email.Should().Be("test@test.com");
            result.User.DisplayName.Should().Be("test");
            result.AccessToken.Should().NotBeEmpty();
            result.RefreshToken.Should().NotBeEmpty();
            result.IdToken.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenUserNotExists()
        {
            // Arrange
            await Factory.ResetDatabaseAsync();
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                DisplayName = "test",
                Email = "test@test.com",
                Password = HashPassword("test"),
                Role = UserRole.User
            };

            await Factory.ExecuteDbContextAsync(async db =>
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            });


            // Act
            var response = await Client.PostAsync(
                "/api/v1/auth/login",
                new StringContent(
                    JsonSerializer.Serialize(new { email = "test@test.com", password = "incorrect-password" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerOptions);

            result.Should().NotBeNull();
            result.Message.Should().Be("Invalid email or password");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
