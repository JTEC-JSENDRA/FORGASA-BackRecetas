using Microsoft.EntityFrameworkCore;

// Crear el constructor de la aplicaci�n web
var builder = WebApplication.CreateBuilder(args);

// Registrar servicios en el contenedor de dependencias
// Agrega soporte para controladores API
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Agrega herramientas para explorar endpoints HTTP y generar documentaci�n Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
// Agrega generador Swagger (documentaci�n interactiva de la API)
builder.Services.AddSwaggerGen();

// Construye la aplicaci�n
var app = builder.Build();

// Configura el middleware de la aplicaci�n dependiendo del entorno
if (app.Environment.IsDevelopment())
{
    // En entorno de desarrollo, habilita Swagger y su UI para probar los endpoints
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirecciona autom�ticamente HTTP a HTTPS
app.UseHttpsRedirection();

// Configura pol�tica CORS para permitir solicitudes de cualquier origen, m�todo y encabezado
app.UseCors(options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// Habilita la autorizaci�n (aunque no hay autenticaci�n configurada en este ejemplo)
app.UseAuthorization();

// Mapea autom�ticamente todos los controladores definidos
app.MapControllers();

// Ejecuta la aplicaci�n web
app.Run();

