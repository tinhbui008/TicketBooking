using TicketBooking.Application;
using TicketBooking.Domain;
using TicketBooking.Domain.Exceptions;
using TicketBooking.Infrastructure;

namespace TicketBooking.Tests;

public class BookingServiceTests
{
    private readonly InMemoryEventRepository _eventRepository = new();
    private readonly InMemoryBookingRepository _bookingRepository = new();
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _service = new BookingService(_eventRepository, _bookingRepository);
    }

    [Fact]
    public async Task CreateAsync_EventNotExist_ShouldThrow()
    {
        await Assert.ThrowsAsync<EventNotFoundException>(() => _service.CreateAsync(Guid.NewGuid(), "A1"));
    }

    [Fact]
    public async Task CreateAsync_AvailableSeat_ShouldHoldSeatAndSaveBooking()
    {
        var evt = await _eventRepository.AddAsync(new Event("Concert-02", new[] { "B1" }));

        var booking = await _service.CreateAsync(evt.Id, "B1");

        Assert.Equal(EBookingStatus.SeatHeld, booking.Status);
        Assert.Equal(ESeatStatus.Held, evt.Seats.Single(s => s.Name == "B1").Status);
        Assert.Same(booking, await _bookingRepository.GetByIdAsync(booking.Id));
    }

    [Fact]
    public async Task CreateAsync_SeatAlreadyHeld_ShouldThrow()
    {
        var evt = await _eventRepository.AddAsync(new Event("Concert-02", new[] { "B1" }));
        await _service.CreateAsync(evt.Id, "B1");

        await Assert.ThrowsAsync<SeatNotAvailableException>(() => _service.CreateAsync(evt.Id, "B1"));
    }
}