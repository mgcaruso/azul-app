-- Prueba de por qué el índice de trigramas no se usa. Correr con los índices temporales creados.
-- Texto: catering (310 coincidencias).

\echo === A) Solo nombre y descripción (sin la rama de servicios)
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*) FROM "Providers" p
WHERE p."Status" = 'Published'
  AND (f_matches(p."DisplayName", '%catering%') OR f_matches(p."Description", '%catering%'));

\echo === B) Las tres ramas como UNION (cada una usa su propio índice)
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*) FROM (
  SELECT p."Id" FROM "Providers" p
   WHERE p."Status" = 'Published' AND f_matches(p."DisplayName", '%catering%')
  UNION
  SELECT p."Id" FROM "Providers" p
   WHERE p."Status" = 'Published' AND f_matches(p."Description", '%catering%')
  UNION
  SELECT p."Id" FROM "Providers" p
   JOIN "ProviderOfferings" po ON po."ProviderId" = p."Id"
   JOIN "Offerings" o ON o."Id" = po."OfferingId"
   WHERE p."Status" = 'Published' AND f_matches(o."Name", '%catering%')
) t;

\echo === C) Consulta original (para comparar)
EXPLAIN (ANALYZE, BUFFERS)
SELECT count(*) FROM "Providers" p
WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", '%catering%') OR f_matches(p."Description", '%catering%') OR EXISTS (
  SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id"
  WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", '%catering%')));
