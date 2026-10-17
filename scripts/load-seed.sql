-- Seed masivo para pruebas de carga. Correr SOLO contra la base azul_massive_load.
-- Requiere que la API ya haya arrancado una vez con el perfil "load" (carga categorías y servicios).
-- Se corre una sola vez; para repetir, vaciar antes: TRUNCATE "Providers" RESTART IDENTITY CASCADE;

\set total 20000

SELECT setseed(0.42);

WITH base AS (
    SELECT
        g,
        random() AS r_type,
        random() AS r_status,
        random() AS r_insta,
        random() AS r_hours,
        random() AS r_photo,
        1 + floor(random() * 20)::int AS i_first,
        1 + floor(random() * 20)::int AS i_last,
        1 + floor(random() * 10)::int AS i_pre,
        1 + floor(random() * 10)::int AS i_suf,
        now() - random() * interval '365 days' AS created
    FROM generate_series(1, :total) g
),
typed AS (
    SELECT *,
        CASE WHEN r_type < 0.70 THEN 'Individual'
             WHEN r_type < 0.90 THEN 'Venture'
             ELSE 'Business' END AS ptype,
        (ARRAY['Lucía','Martín','Sofía','Mateo','Valentina','Santiago','Camila','Joaquín','Julieta','Nicolás',
               'Agustina','Facundo','Micaela','Lucas','Florencia','Tomás','Romina','Gonzalo','Paula','Diego'])[i_first] AS first_name,
        (ARRAY['Gómez','Rodríguez','Fernández','López','Martínez','Pérez','Sánchez','Romero','Díaz','Álvarez',
               'Torres','Ruiz','Ramírez','Flores','Acosta','Benítez','Medina','Herrera','Suárez','Aguirre'])[i_last] AS last_name,
        (ARRAY['Taller','Estudio','Casa','Servicios','Atelier','Punto','Centro','Rincón','Mundo','Espacio'])[i_pre] AS prefix,
        (ARRAY['Azul','Sur','Norte','Plus','Pro','Express','Hogar','Creativo','Verde','Central'])[i_suf] AS suffix
    FROM base
)
INSERT INTO "Providers"
    ("Type", "DisplayName", "FirstName", "LastName", "Description", "PhotoKey", "PhoneNumber",
     "InstagramUsername", "Address", "Hours", "Status", "FirstPublishedAt", "CreatedAt", "UpdatedAt")
SELECT
    ptype,
    CASE WHEN ptype = 'Individual' THEN first_name || ' ' || last_name ELSE prefix || ' ' || suffix END,
    CASE WHEN ptype = 'Individual' THEN first_name END,
    CASE WHEN ptype = 'Individual' THEN last_name END,
    'Servicio de prueba número ' || g || '. Atención con turno previo en la zona de Azul.',
    CASE WHEN r_photo < 0.5 THEN 'seed-' || g || '.jpg' END,
    '+549' || lpad(floor(random() * 10000000000)::bigint::text, 10, '0'),
    CASE WHEN r_insta < 0.6 THEN 'seed_user_' || g END,
    CASE WHEN ptype = 'Business' THEN 'Av. Mitre ' || (100 + g % 2900) END,
    CASE WHEN r_hours < 0.5 THEN 'Lunes a viernes de 9 a 18' END,
    CASE WHEN r_status < 0.90 THEN 'Published'
         WHEN r_status < 0.95 THEN 'Pending'
         WHEN r_status < 0.98 THEN 'Suspended'
         ELSE 'Rejected' END,
    CASE WHEN r_status < 0.90 THEN created END,
    created,
    created
FROM typed;

-- De 1 a 3 servicios por ficha, al azar entre los que cargó la API.
WITH ids AS (SELECT array_agg("Id") AS a FROM "Offerings")
INSERT INTO "ProviderOfferings" ("ProviderId", "OfferingId", "CreatedAt")
SELECT DISTINCT p."Id", ids.a[1 + floor(random() * array_length(ids.a, 1))::int], now()
FROM "Providers" p
CROSS JOIN ids
CROSS JOIN generate_series(1, 1 + (p."Id" % 3)) k;

ANALYZE;

SELECT "Status", count(*) FROM "Providers" GROUP BY "Status" ORDER BY 2 DESC;
SELECT (SELECT count(*) FROM "Providers") AS providers, (SELECT count(*) FROM "ProviderOfferings") AS provider_offerings;
