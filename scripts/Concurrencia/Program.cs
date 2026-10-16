// Prueba de concurrencia del alta pública: dispara N POST /api/providers a la vez y cuenta las respuestas.
// Requiere la API corriendo en local y el límite por IP desactivado.
//
// Uso (desde scripts/Concurrencia):
//   dotnet run -- 1            mismo Instagram: esperado 1x201 y 19x409
//   dotnet run -- 2            mismo celular, sin Instagram: esperado 20x201
//   dotnet run -- 3            datos distintos: esperado 20x201
//   dotnet run -- 1 50 4       caso 1, 50 pedidos, servicio con id 4

using System.Net.Http.Json;

var caso = args.Length > 0 ? int.Parse(args[0]) : 1;
var cantidad = args.Length > 1 ? int.Parse(args[1]) : 20;
var offeringId = args.Length > 2 ? int.Parse(args[2]) : 1;
var url = args.Length > 3 ? args[3] : "http://localhost:5297/api/providers";

var sufijo = DateTime.Now.ToString("HHmmss"); // evita chocar con corridas anteriores
using var client = new HttpClient();

object BuildBody(int i) => caso switch
{
    1 => new
    {
        type = "Individual", firstName = "Prueba", lastName = "Concurrencia", displayName = $"Prueba Concurrencia {sufijo}", phoneNumber = "2241555123",
        instagramUsername = $"prueba_conc_{sufijo}", offeringIds = new[] { offeringId }, acceptedTerms = true
    },
    2 => new
    {
        type = "Individual", firstName = "Prueba", lastName = "Concurrencia", displayName = $"Prueba Concurrencia {sufijo}", phoneNumber = "2241555123",
        offeringIds = new[] { offeringId }, acceptedTerms = true
    },
    _ => new
    {
        type = "Individual", firstName = "Prueba", lastName = "Concurrencia", displayName = $"Prueba Concurrencia {sufijo} {i}", phoneNumber = $"22415{i:D5}",
        instagramUsername = $"prueba_conc_{sufijo}_{i}", offeringIds = new[] { offeringId }, acceptedTerms = true
    }
};

// Todos esperan la señal y arrancan juntos.
var salida = new TaskCompletionSource();
var tareas = Enumerable.Range(1, cantidad).Select(async i =>
{
    await salida.Task;
    try
    {
        var response = await client.PostAsJsonAsync(url, BuildBody(i));
        var texto = await response.Content.ReadAsStringAsync();
        return (((int)response.StatusCode).ToString(), texto);
    }
    catch (Exception ex)
    {
        return ($"error: {ex.GetType().Name}", ex.Message);
    }
}).ToList();

salida.SetResult();
var resultados = await Task.WhenAll(tareas);

var informe = new System.Text.StringBuilder();
informe.AppendLine($"Caso {caso}, {cantidad} pedidos, {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
informe.AppendLine($"URL: {url}");
informe.AppendLine();
informe.AppendLine("Resumen:");
foreach (var grupo in resultados.GroupBy(r => r.Item1).OrderBy(g => g.Key))
{
    informe.AppendLine($"  {grupo.Key}: {grupo.Count()}");
}

informe.AppendLine();
informe.AppendLine("Detalle de las respuestas que no fueron 201 (una por código):");
foreach (var grupo in resultados.Where(r => r.Item1 != "201").GroupBy(r => r.Item1))
{
    informe.AppendLine($"  [{grupo.Key}] {grupo.First().Item2}");
}

informe.AppendLine();
informe.AppendLine($"Ahora mirá en la base cuántas fichas 'Prueba Concurrencia {sufijo}' quedaron y borralas.");

Directory.CreateDirectory("resultados");
var archivo = Path.Combine("resultados", $"caso-{caso}-{sufijo}.txt");
File.WriteAllText(archivo, informe.ToString());

Console.WriteLine(informe);
Console.WriteLine($"Guardado en: {Path.GetFullPath(archivo)}");
