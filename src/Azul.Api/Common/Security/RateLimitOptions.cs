namespace Azul.Api.Common.Security;

public class RateLimitOptions
{
        public int AdminLoginPerMinute { get; set; } = 5;
        public int AdminLoginGlobalPerMinute { get; set; } = 30;
        public int SignupPerHour { get; set; } = 5;
        public int PhotoUploadPerHour { get; set; } = 10;
        
        
}