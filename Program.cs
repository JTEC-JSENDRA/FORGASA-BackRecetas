using Microsoft.EntityFrameworkCore;

// Crear el constructor de la aplicación web
var builder = WebApplication.CreateBuilder(args);

// Registrar servicios en el contenedor de dependencias
// Agrega soporte para controladores API
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Agrega herramientas para explorar endpoints HTTP y generar documentación Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
// Agrega generador Swagger (documentación interactiva de la API)
builder.Services.AddSwaggerGen();

// Construye la aplicación
var app = builder.Build();

// Configura el middleware de la aplicación dependiendo del entorno
if (app.Environment.IsDevelopment())
{
    // En entorno de desarrollo, habilita Swagger y su UI para probar los endpoints
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirecciona automáticamente HTTP a HTTPS
app.UseHttpsRedirection();

// Configura política CORS para permitir solicitudes de cualquier origen, método y encabezado
app.UseCors(options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// Habilita la autorización (aunque no hay autenticación configurada en este ejemplo)
app.UseAuthorization();

// Mapea automáticamente todos los controladores definidos
app.MapControllers();

// Ejecuta la aplicación web
app.Run();

