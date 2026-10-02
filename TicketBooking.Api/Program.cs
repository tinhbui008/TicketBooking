using System.Text.Json.Serialization;
using TicketBooking.Application;
using TicketBooking.Domain;
using TicketBooking.Domain.Exceptions;
using TicketBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/events", async (IEventRepository repo) =>
{
    var events = await repo.GetAllAsync();
    return events.Select(EventResponse.From);
});

app.MapPost("/bookings", async (CreateBookingRequest req, BookingService svc) =>
{
    if (req.EventId == Guid.Empty)
        return Results.BadRequest("EventId is required");
    if (string.IsNullOrWhiteSpace(req.SeatName))
        return Results.BadRequest("SeatName is required");

    try
    {
        var booking = await svc.CreateAsync(req.EventId, req.SeatName);
        return Results.Created($"/bookings/{booking.Id}", BookingResponse.From(booking));
    }
    catch (EventNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    } // event không có → 404
    catch (SeatNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    } // ghế không có → 404
    catch (SeatNotAvailableException ex)
    {
        return Results.Conflict(ex.Message);
    } // ghế bận → 409
});

app.MapGet("/bookings/{id:guid}", async (Guid id, IBookingRepository repo) =>
{
    var b = await repo.GetByIdAsync(id);
    return b is null ? Results.NotFound() : Results.Ok(BookingResponse.From(b));
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.Run();


record CreateBookingRequest(Guid EventId, string SeatName);

record SeatResponse(string Name, ESeatStatus Status);

record EventResponse(Guid Id, string Name, IEnumerable<SeatResponse> Seats)
{
    public static EventResponse From(Event evt) =>
        new(evt.Id, evt.Name, evt.Seats.Select(s => new SeatResponse(s.Name, s.Status)));
}

record BookingResponse(Guid Id, Guid EventId, string SeatName, EBookingStatus Status)
{
    public static BookingResponse From(Booking booking) =>
        new(booking.Id, booking.EventId, booking.SeatName, booking.Status);
}