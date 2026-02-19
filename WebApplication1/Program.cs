using System.Text.Json;
using Corvus.Json;
using UCP.Model.Discovery;
using UCP.Model.Schemas.Service;
using UCP.Model.Schemas.Shopping;


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
app.MapPost("/checkout-sessions", async (HttpRequest request) =>
{

    using JsonDocument document = await JsonDocument.ParseAsync(request.Body);
    // CheckoutCreateRequest checkout = new CheckoutCreateRequest(document.RootElement);
    UnifiedCheckoutCreateRequest unifiedCheckoutCreateRequest = new UnifiedCheckoutCreateRequest(document);
    // BuyerConsent buyer = new BuyerConsent.Create();
    // BuyerConsent buyerConsent = BuyerConsent.Create(null, null, null, null);
    var result = unifiedCheckoutCreateRequest.BuyerConsent.Validate(ValidationContext.ValidContext, ValidationLevel.Detailed);
    // TODO: Implement Logic
    
    if (result.IsValid) return Results.Created();
    var validationErrors = new Dictionary<string, string[]>();
    foreach (ValidationResult error in result.Results)
    {
        validationErrors.Add(error.Location.ToString(), [error.Message]);
    }
    
    return TypedResults.ValidationProblem(validationErrors);
}).Accepts<UnifiedCheckoutCreateRequest>("application/json");

app.Run();
public record CheckoutDto(string hello);
public class AppStateService
{
    public Guid UUID { get; set; } = Guid.NewGuid();
}

public class UnifiedCheckoutCreateRequest
{
    public BuyerConsent BuyerConsent {get; set;}
    public UnifiedCheckoutCreateRequest(JsonDocument document)
    {
        this.BuyerConsent = new BuyerConsent(document.RootElement);
    }
}



