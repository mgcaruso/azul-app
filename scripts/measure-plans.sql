-- Plan de ejecución (EXPLAIN ANALYZE) del count de /api/search para cada palabra.
-- Sirve para ver cuántas filas recorre (Rows Removed by Filter + rows) y qué tipo de scan usa.

\echo === catering
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%catering%')) OR f_matches(p."Description", lower('%catering%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%catering%'))));

\echo === reparaciones de celulares
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%reparaciones de celulares%')) OR f_matches(p."Description", lower('%reparaciones de celulares%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%reparaciones de celulares%'))));

\echo === celulares
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%celulares%')) OR f_matches(p."Description", lower('%celulares%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%celulares%'))));

\echo === repar
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%repar%')) OR f_matches(p."Description", lower('%repar%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%repar%'))));

\echo === 12345
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%12345%')) OR f_matches(p."Description", lower('%12345%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%12345%'))));

\echo === gomez
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%gomez%')) OR f_matches(p."Description", lower('%gomez%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%gomez%'))));

\echo === taller
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%taller%')) OR f_matches(p."Description", lower('%taller%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%taller%'))));

\echo === Lucía
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%Lucía%')) OR f_matches(p."Description", lower('%Lucía%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%Lucía%'))));

\echo === atencion
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%atencion%')) OR f_matches(p."Description", lower('%atencion%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%atencion%'))));

\echo === azul
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%azul%')) OR f_matches(p."Description", lower('%azul%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%azul%'))));

\echo === jo
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%jo%')) OR f_matches(p."Description", lower('%jo%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%jo%'))));

\echo === zzzz
EXPLAIN (ANALYZE, BUFFERS) SELECT count(*) FROM "Providers" p WHERE p."Status" = 'Published' AND (f_matches(p."DisplayName", lower('%zzzz%')) OR f_matches(p."Description", lower('%zzzz%')) OR EXISTS (SELECT 1 FROM "ProviderOfferings" p0 JOIN "Offerings" o ON p0."OfferingId" = o."Id" WHERE p."Id" = p0."ProviderId" AND f_matches(o."Name", lower('%zzzz%'))));
