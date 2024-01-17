using PdfConverter.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PdfManipulationService>();
builder.Services.AddScoped<CompressService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Replace "YOUR_LICENSE_KEY" with the actual license key you received
string licenseKey = "IRONSUITE.VOVA2010201040.GMAIL.COM.22259-52B2B5B9C7-D6YTI-NTDNYYA7ZYGN-EPOKLXOGFPZJ-GSGCGR5OMBZC-TZOTM2S57R4K-4L5OJ3E7T6WP-VGSZ7SVGUMNK-SJH4MA-TA77MHECWR6LUA-DEPLOYMENT.TRIAL-NRXA4S.TRIAL.EXPIRES.16.FEB.2024";

License.LicenseKey = licenseKey;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();