using Azul.Api.Tests.Auth;

namespace Azul.Api.Tests;

// Todas las clases de tests comparten una sola API y una sola base (azul_tests).
// Sin esto, xUnit corre cada clase en paralelo y cada una borra y recrea la base de las otras.
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<AuthApiFactory>
{
    public const string Name = "api";
}
