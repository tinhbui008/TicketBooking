using TicketBooking.Application;
using TicketBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/events", async (IEventRepository repo) => await repo.GetAllAsync());

app.MapPost("/bookings", async (CreateBookingRequest req, BookingService svc) =>
{
    try
    {
        var booking = await svc.Create(req.EventId, req.SeatName);
        return Results.Created($"/bookings/{booking.Id}", booking);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    } // event không có → 404
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    } // ghế bận → 409
});

app.MapGet("/bookings/{id:guid}", async (Guid id, IBookingRepository repo) =>
{
    var b = await repo.GetByIdAsync(id);
    return b is null ? Results.NotFound() : Results.Ok(b);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.Run();


record CreateBookingRequest(Guid EventId, string SeatName);