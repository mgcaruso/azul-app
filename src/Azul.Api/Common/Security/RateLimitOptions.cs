namespace Azul.Api.Common.Security;

public class RateLimitOptions
{
        public int SignupPerHour { get; set; } = 5;
        public int PhotoUploadPerHour { get; set; } = 10;
        public int GoogleSignInPerMinute { get; set; } = 10;
        public int PublicPerMinute { get; set; } = 300;
        // Búsquedas de texto (/api/search y /api/offerings): corren consultas más caras que una lectura común.
        public int SearchPerMinute { get; set; } = 60;
        
        
}