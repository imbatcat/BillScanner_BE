using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BillScanner.Controllers.Base;
using Business.Handlers.Authentication.Register.Dto;
using BillScanner.Models;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Test.Configuration;
using Test.Integration.Business.Handlers.BaseTests;
using Xunit.Abstractions;

namespace Test.Integration.Business.Handlers.Authentication
{
    [Collection("BillScannerTestCollection")]
    public class RegisterHandlerTest(
        CustomWebApplicationFactory factory,
        ITestOutputHelper outputHelper)
        : BaseTest(factory, outputHelper)
    {
        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenUserDoesNotExist()
        {
            // Arrange
            await Factory.ResetDatabaseAsync();

            // Act
            var response = await Client.PostAsync(
                "/api/v1/auth/register",
                new StringContent(
                    JsonSerializer.Serialize(new { email = "test@test.com", password = "test", displayName = "test" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegisterResponse>(responseContent, JsonSerializerOptions);

            result.Should().NotBeNull();
            result.User.Email.Should().Be("test@test.com");
            result.AccessToken.Should().NotBeEmpty();
            result.RefreshToken.Should().NotBeEmpty();
            result.IdToken.Should().NotBeEmpty();

            await Factory.ExecuteDbContextAsync(async db =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
                user.Should().NotBeNull();
                user.DisplayName.Should().Be("test");
            });
        }

        [Fact]
        public async Task Register_ShouldReturnConflict_WhenUserAlreadyExists()
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
                "/api/v1/auth/register",
                new StringContent(
                    JsonSerializer.Serialize(new { email = "test@test.com", password = "test", displayName = "test" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ErrorResponse>(responseContent, JsonSerializerOptions);

            result.Should().NotBeNull();
            result.Message.Should().Be("A record with this information already exists");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}