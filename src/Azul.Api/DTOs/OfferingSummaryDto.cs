namespace Azul.Api.DTOs;

// Versión corta de un servicio, para listas dentro de una categoría:
// la categoría ya viene en la URL, así que no se repite.
public class OfferingSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
