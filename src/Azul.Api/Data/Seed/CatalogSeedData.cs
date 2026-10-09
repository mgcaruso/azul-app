using Azul.Api.Entities;

namespace Azul.Api.Data.Seed;

public static class CatalogSeedData
{
    public static readonly Dictionary<string, string[]> Catalog = new()
    {
        ["Comida"] =
        [
            "Tortas por encargo", "Tortas de cumpleaños", "Viandas", "Viandas saludables", "Catering",
            "Pastelería", "Panadería casera", "Empanadas", "Pizzas caseras", "Comida sin TACC",
            "Mesa dulce", "Alfajores artesanales", "Pastas caseras", "Comida vegana"
        ],
        ["Hogar y reparaciones"] =
        [
            "Plomería", "Destapaciones", "Gasista matriculado", "Electricista", "Cerrajería",
            "Pintura de interiores", "Albañilería", "Arreglo de techos", "Herrería", "Carpintería",
            "Instalación de aire acondicionado", "Service de lavarropas", "Arreglo de heladeras",
            "Colocación de durlock", "Impermeabilización"
        ],
        ["Limpieza"] =
        [
            "Limpieza de casas", "Limpieza de oficinas", "Limpieza de tapizados", "Limpieza de alfombras",
            "Limpieza de vidrios", "Limpieza final de obra", "Limpieza de tanques de agua"
        ],
        ["Jardín"] =
        [
            "Corte de pasto", "Jardinería", "Poda de árboles", "Paisajismo", "Mantenimiento de piletas",
            "Fumigación", "Riego automático"
        ],
        ["Tecnología"] =
        [
            "Arreglo de notebooks", "Arreglo de celulares", "Cambio de pantalla de celular", "Arreglo de PC",
            "Instalación de redes wifi", "Cámaras de seguridad", "Diseño de páginas web",
            "Recuperación de datos", "Clases de computación para adultos mayores"
        ],
        ["Autos y motos"] =
        [
            "Mecánica en general", "Gomería", "Electricidad del automotor", "Chapa y pintura",
            "Lavado de autos", "Service de motos", "Auxilio mecánico", "Alineación y balanceo",
            "Polarizado"
        ],
        ["Belleza"] =
        [
            "Peluquería", "Peluquería a domicilio", "Barbería", "Manicuría", "Uñas esculpidas",
            "Depilación", "Maquillaje", "Pestañas", "Masajes", "Cosmetología"
        ],
        ["Mascotas"] =
        [
            "Paseador de perros", "Peluquería canina", "Veterinaria a domicilio", "Adiestramiento de perros",
            "Guardería de mascotas", "Cuidado de gatos"
        ],
        ["Clases"] =
        [
            "Clases de inglés", "Apoyo escolar", "Clases de matemática", "Clases de guitarra",
            "Clases de piano", "Clases de canto", "Clases de yoga", "Entrenador personal",
            "Clases de natación", "Clases de dibujo", "Clases de cocina"
        ],
        ["Eventos"] =
        [
            "Fotógrafo", "DJ", "Animación de cumpleaños infantiles", "Alquiler de inflables",
            "Decoración de eventos", "Alquiler de vajilla", "Mozos para eventos", "Video de casamientos",
            "Shows de magia"
        ],
        ["Mudanzas y fletes"] =
        [
            "Fletes", "Mudanzas", "Armado de muebles", "Guardamuebles", "Retiro de escombros"
        ],
        ["Salud y cuidado"] =
        [
            "Cuidado de adultos mayores", "Enfermería a domicilio", "Kinesiología", "Acompañante terapéutico",
            "Niñera", "Nutricionista", "Psicología online"
        ]
    };

    public static readonly string[] FirstNames =
    [
        "María", "José", "Lucía", "Juan", "Sofía", "Martín", "Valentina", "Diego", "Camila", "Pablo",
        "Florencia", "Nicolás", "Agustina", "Sebastián", "Julieta", "Matías", "Carolina", "Federico",
        "Romina", "Gustavo", "Silvia", "Marcelo", "Natalia", "Facundo", "Lorena", "Hernán", "Paula",
        "Leandro", "Gabriela", "Ezequiel", "Mariana", "Ramiro", "Andrea", "Tomás", "Verónica", "Lu"
    ];

    public static readonly string[] LastNames =
    [
        "González", "Rodríguez", "Gómez", "Fernández", "López", "Díaz", "Martínez", "Pérez", "García",
        "Sánchez", "Romero", "Sosa", "Álvarez", "Torres", "Ruiz", "Ramírez", "Flores", "Acosta",
        "Benítez", "Medina", "Herrera", "Aguirre", "Pereyra", "Gutiérrez", "Giménez", "Molina",
        "Silva", "Castro", "Rojas", "Ortiz", "Núñez", "Luna", "Juárez", "Cabrera", "Ríos", "Ferreyra"
    ];

    public static readonly (string Format, ProviderType Type)[] NameFormats =
    [
        ("{0} {1}", ProviderType.Individual), ("{0} {1}", ProviderType.Individual),
        ("{0} {1}", ProviderType.Individual), ("{0} {1}", ProviderType.Individual),
        ("{0} a domicilio", ProviderType.Individual),
        ("Taller {1}", ProviderType.Business), ("Lo de {0}", ProviderType.Business),
        ("Hnos. {1}", ProviderType.Business), ("{1} Servicios", ProviderType.Business),
        ("Las cosas de {0}", ProviderType.Business)
    ];
}
