# Búsqueda de texto: índices al escalar

Estado (17/10): **no hay índices de texto en `Providers`** y el filtro de texto es el simple (`OR` con `f_matches` + `EXISTS`). Esto es la referencia para el día que la búsqueda se ponga lenta. No hace falta tocar nada hasta entonces.

## Cuándo retomarlo
- `GET /api/search` con texto tarda más de ~50 ms en producción (se ve en Seq), o
- hay unas 5.000 fichas publicadas.

## Qué se midió (base `azul_massive_load`, 20.000 fichas, 18.043 publicadas)
| | Sin índices | Con GIN + `UNION` |
|---|---|---|
| `count` (`catering`) | ~85 ms | 9 ms |
| Página (`catering`) | ~95-120 ms | 37 ms |
| Request completa (`catering`) | ~195-217 ms | 66 ms |
| Request completa (`hola`) | - | 40 ms |

En SQL puro: 1-2 ms para palabras selectivas, ~51-55 ms para palabras que coinciden con casi todo, ~77 ms con 2 letras (sin trigramas no hay índice que ayude). Detalle en `mediciones/`.

## Lo importante: los dos cambios van juntos
1. **Índices GIN de trigramas en `Providers`** (migración nueva; misma expresión que usa `f_matches`):

```sql
CREATE INDEX "IX_Providers_DisplayName_Search" ON "Providers"
    USING gin (f_unaccent(lower("DisplayName")) gin_trgm_ops);
CREATE INDEX "IX_Providers_Description_Search" ON "Providers"
    USING gin (f_unaccent(lower("Description")) gin_trgm_ops);
```

2. **Reescribir `WhereMatchesText` en `ProviderFilters.cs` como `UNION` de ids** en lugar de un `OR`. Con solo los índices, Postgres no los usa: el `OR` con la rama del `EXISTS` lo obliga a escanear todo (variantes A/B/C en `mediciones/variantes.txt`). Con el `UNION`, cada rama usa su índice:

```csharp
var porNombre = providers.Where(p => SearchText.Matches(p.DisplayName, pattern)).Select(p => p.Id);
var porDescripcion = providers.Where(p => SearchText.Matches(p.Description, pattern)).Select(p => p.Id);
var porServicio = providers.SelectMany(p => p.Offerings)
    .Where(po => SearchText.Matches(po.Offering.Name, pattern))
    .Select(po => po.ProviderId);

var ids = porNombre.Union(porDescripcion).Union(porServicio);
providers = providers.Where(p => ids.Contains(p.Id));
```

(Probado en C# el 17/10 con el perfil `load`: ver tabla de arriba.) El `UNION` sin los índices no mejora nada, y los índices sin el `UNION` tampoco.

## Otros puntos
- `IX_Offerings_Name_Search` se quitó con la migración `QuitarIndiceBusquedaOfferings` (17/10): con tablas chicas no aporta. Se mantienen `pg_trgm` y `f_unaccent`.
- Después de los índices, lo más caro pasa a ser la query de la página (ordenar por completitud + `ROW_NUMBER` de las ofertas). Medirla aparte.
- Idea no decidida: omitir el `count` si la página no viene llena (`Total = (Page - 1) * PageSize + items.Count`). El front usa `Total`.
- Más allá: `tsvector` o un motor de búsqueda aparte (Typesense, Meilisearch). Para Azul alcanza con Postgres.

## Cómo repetir la medición
Base `azul_massive_load` cargada con `scripts/load-seed.sql`, perfil `load` en `launchSettings.json` (`dotnet run --launch-profile load`), scripts `scripts/measure-*.sql` y Seq para ver el tiempo de cada comando. Palabras: "catering", "reparaciones de celulares", un número de ficha, "azul", "atención", "gomez", "lucia", "taller", "jo", "zzzz".
