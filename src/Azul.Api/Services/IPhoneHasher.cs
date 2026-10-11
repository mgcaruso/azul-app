namespace Azul.Api.Services;

public interface IPhoneHasher
{
    string Hash(string phoneNumber);
}
