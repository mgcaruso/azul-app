namespace Azul.Api.DTOs.Categories;

/// <summary>La categoría de otra entidad: solo lo necesario para mostrarla y enlazarla.</summary>
public class CategoryRefDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
