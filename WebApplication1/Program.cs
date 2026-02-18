using Corvus.Json;
using Corvus.Json.Internal;
using UCP.Model.Discovery;
using UCP.Model.Schemas.Shopping;
// JsonValueConverter.EnableInefficientDeserializationSupport = true;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddHttpLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(x => x.FullName);
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
    app.UseSwagger();
    app.UseSwaggerUI();

}
app.Use((context, next) =>
{
    Console.Write(context.Request.Body);
    return next(context);
});
// app.UseHttpsRedirection();
app.UseRouting();

app.MapGet("/", () => "Hello World!");
app.MapGet("/.well-known/ucp", (HttpContext context, AppStateService appStateService) =>
{
    string template = "";
    string baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/";

    try
    {
        // Open the text file using a stream reader.
        using StreamReader reader = new("discovery_profile.json");

        // Read the stream as a string.
        template = reader.ReadToEnd();

    }
    catch (IOException e)
    {
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
    }
    string jsonString = template.Replace("{{ENDPOINT}}", baseUrl).Replace("{{SHOP_ID}}", appStateService.UUID.ToString());
    BusinessProfile businessProfile = BusinessProfile.Parse(jsonString);
    if (businessProfile.IsValid())
    {
        
    }

    
    return TypedResults.Ok(businessProfile);

});
app.MapPost("/checkout-sessions", (Checkout checkout) =>
{
    // TODO: Implement Logic
    if (checkout.IsValid()) return Results.Created();
    return Results.UnprocessableEntity();
});

app.Run();
public record CheckoutDto(string hello);
public class AppStateService
{
    public Guid UUID { get; set; } = Guid.NewGuid();
}



