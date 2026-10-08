using System.ComponentModel.DataAnnotations;

namespace Azul.Api.DTOs;

public class CategorySaveDto
{
    // Sin el "required" de C#: si no, cuando falta "name" falla la lectura del JSON
    // y nunca llegan a correr estas validaciones. [Required] igual rechaza null y "".
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
