using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;
using Xunit;

namespace SaaS.API.IntegrationTests.Auth;

public class LoginEndpointTests : IClassFixture<LoginWebApplicationFactory>
{
    private readonly LoginWebApplicationFactory _factory;

    public LoginEndpointTests(LoginWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest("admin@example.com", "123456"))
        };
        request.Headers.Add("X-Tenant-Id", LoginWebApplicationFactory.ValidTenantId);

        using var response = await client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<Result<LoginResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.Succeeded);
        Assert.NotNull(payload.Data);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.Data.RefreshToken));
        Assert.True(payload.Data.AccessTokenExpiresAtUtc > DateTime.UtcNow);
        Assert.True(payload.Data.RefreshTokenExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenTenantHeaderIsMissing()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@example.com", "123456"));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Missing tenant context.", problem!.Title);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest("not-an-email", "123456"))
        };
        request.Headers.Add("X-Tenant-Id", LoginWebApplicationFactory.ValidTenantId);

        using var response = await client.SendAsync(request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest("admin@example.com", "wrong-password"))
        };
        request.Headers.Add("X-Tenant-Id", LoginWebApplicationFactory.ValidTenantId);

        using var response = await client.SendAsync(request);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Invalid credentials.", problem!.Detail);
    }
}

public class LoginWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ValidTenantId = "tenant_test";
    private static readonly Guid SessionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITenantService>();
            services.RemoveAll<IIdentityService>();

            services.AddScoped<ITenantService, TestTenantService>();
            services.AddScoped<IIdentityService, TestIdentityService>();
        });
    }

    private sealed class TestTenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentTenantId()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        }

        public string? GetConnectionString()
        {
            return null;
        }
    }

    private sealed class TestIdentityService : IIdentityService
    {
        public Task<(Result Result, AuthUserInfo? User)> AuthenticateAsync(string email, string password)
        {
            if (email == "admin@example.com" && password == "123456")
            {
                return Task.FromResult<(Result, AuthUserInfo?)>((Result.Success(), new AuthUserInfo("user_test", email)));
            }

            return Task.FromResult<(Result, AuthUserInfo?)>((Result.Failure("Invalid credentials."), null));
        }

        public Task<(Result Result, AuthUserInfo? User)> GetByRefreshTokenAsync(string refreshToken)
        {
            throw new NotSupportedException();
        }

        public Task<UserProfileResponse?> GetProfileAsync(string userId, string tenantId)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<UserSessionResponse>> GetSessionsAsync(string userId, Guid? currentSessionId)
        {
            throw new NotSupportedException();
        }

        public Task<(Result Result, Guid? SessionId)> CreateSessionAsync(string userId, string refreshToken, DateTime expiresAtUtc)
        {
            return Task.FromResult<(Result, Guid?)>((Result.Success(), SessionId));
        }

        public Task<Result> RotateRefreshTokenAsync(string currentRefreshToken, string newRefreshToken, DateTime expiresAtUtc)
        {
            throw new NotSupportedException();
        }

        public Task<Result> RevokeCurrentSessionAsync(string userId, Guid sessionId)
        {
            throw new NotSupportedException();
        }

        public Task<Result> RevokeSessionAsync(string userId, Guid sessionId)
        {
            throw new NotSupportedException();
        }

        public Task<Result> RevokeAllSessionsAsync(string userId)
        {
            throw new NotSupportedException();
        }

        public Task<SessionCacheEntry?> GetSessionStateAsync(string userId, Guid sessionId)
        {
            throw new NotSupportedException();
        }

        public Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName)
        {
            throw new NotSupportedException();
        }
    }
}





