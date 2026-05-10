using MinimalApi_Auth.Data;
using MinimalApi_Auth.Services;
using System.Security.Claims;
using MinimalApi_Auth.Models;


namespace MinimalApi_Auth.Models.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // ── POST /api/auth/register ──────────────────────────
            app.MapPost("/api/auth/register", (RegisterRequest req, UserStore store, HashService hasher) =>
            {
                if (string.IsNullOrWhiteSpace(req.Username) || req.Username.Length < 3)
                    return Results.BadRequest(new { error = "Username must be at least 3 characters." });

                if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
                    return Results.BadRequest(new { error = "Password must be at least 8 characters." });

                var user = new User
                {
                    Username = req.Username,
                    PasswordHash = hasher.HashPassword(req.Password),
                    Role = "Player"
                };

                if (!store.TryAdd(user))
                    return Results.Conflict(new { error = "Username already exists." });

                return Results.Ok(new { message = "User registered successfully." });
            });

            // ── POST /api/auth/login ─────────────────────────────
            app.MapPost("/api/auth/login", (LoginRequest req, UserStore store, HashService hasher, JwtService jwt) =>
            {
                if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                    return Results.BadRequest(new { error = "Username and password are required." });

                var user = store.FindByUsername(req.Username);

                if (user is null)
                    return Results.Unauthorized();

                if (user.IsLockedOut)
                    return Results.Json(new { error = "Account is locked. Too many failed attempts." }, statusCode: 403);

                if (!hasher.VerifyPassword(req.Password, user.PasswordHash))
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 3)
                        user.IsLockedOut = true;

                    store.Update(user);
                    return Results.Unauthorized();
                }

                // Success — reset counter
                user.FailedLoginAttempts = 0;
                store.Update(user);

                var token = jwt.GenerateToken(user);
                return Results.Ok(new { token, username = user.Username, role = user.Role });
            });

            // ── GET /api/auth/me ─────────────────────────────────
            app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
            {
                var username = principal.FindFirstValue(ClaimTypes.Name);
                var role = principal.FindFirstValue(ClaimTypes.Role);
                var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                return Results.Ok(new { id, username, role });

            }).RequireAuthorization();
        }
    }
}
