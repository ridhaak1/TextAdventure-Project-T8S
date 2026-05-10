using System.Security.Claims;

namespace MinimalApi_Auth.Models.Endpoints
{
    public static class KeysEndpoints
    {

        public static void MapKeysEndpoints(this WebApplication app)
        {
            // ── GET /api/keys/keyshare/{roomId} ──────────────────
            app.MapGet("/api/keys/keyshare/{roomId}",
             (string roomId, ClaimsPrincipal principal, IConfiguration config) =>
             {
                 var role = principal.FindFirstValue(ClaimTypes.Role);
                 var keyshare = config[$"Keyshares:{roomId}"];

                 if (keyshare is null)
                     return Results.NotFound(new { error = "Room not found." });

                 if (roomId == "room_godmode" && role != "Admin")
                     return Results.Json(new { error = "Access denied. Admin only." }, statusCode: 403);

                 return Results.Ok(new { roomId, keyshare });

             }).RequireAuthorization();
        }
    }
}
