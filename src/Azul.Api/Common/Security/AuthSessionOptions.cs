namespace Azul.Api.Common.Security;

public class AuthSessionOptions
{
    public int ExpireDays { get; set; } = 14;

    public int SecurityStampCheckMinutes { get; set; } = 5;
}