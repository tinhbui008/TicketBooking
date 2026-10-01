using TicketBooking.Application;
using TicketBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/events", async (Guid id, string seatName, BookingService services) =>
{
    await services.Create(id, seatName);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.Run();