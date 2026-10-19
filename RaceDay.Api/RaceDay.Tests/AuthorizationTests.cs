
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Authorization;
using Xunit;

namespace RaceDay.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task UnauthenticatedUser_ReturnsUnauthorized()
    {
        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        var context = CreateAuthorizationContext(httpContext);
        var attribute = new SessionAuthorizeAttribute();

        await attribute.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task ParticipantAccessingOrganiserEndpoint_ReturnsForbidden()
    {
        var session = new TestSession();
        session.SetInt32("UserId", 1);
        session.SetString("UserRole", "Participant");

        var httpContext = new DefaultHttpContext
        {
            Session = session
        };

        var context = CreateAuthorizationContext(httpContext);
        var attribute = new SessionAuthorizeAttribute("Organiser");

        await attribute.OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    private static AuthorizationFilterContext CreateAuthorizationContext(
        HttpContext httpContext)
    {
        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        return new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id => "test-session";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public bool TryGetValue(string key, out byte[]? value)
            => _store.TryGetValue(key, out value);

        public void Set(string key, byte[] value)
            => _store[key] = value;

        public void Remove(string key)
            => _store.Remove(key);

        public void SetInt32(string key, int value)
            => Set(key, BitConverter.GetBytes(value));

        public void SetString(string key, string value)
            => Set(key, System.Text.Encoding.UTF8.GetBytes(value));
    }
}
