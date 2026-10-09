namespace Azul.Api.Entities;

// Si la ficha es de una persona ("María González") o de un negocio ("Taller Gómez").
// En la base se guarda como texto (ver AppDbContext), así se lee directo en una consulta.
public enum ProviderType
{
    Individual = 1,
    Business = 2
}
