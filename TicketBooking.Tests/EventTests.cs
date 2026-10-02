using TicketBooking.Domain;
using TicketBooking.Domain.Exceptions;

namespace TicketBooking.Tests;

public class EventTests
{
    [Fact]
    public void EnsureSeatCanBeHeld_AvailableSeat_ShouldNotThrow()
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        var exception = Record.Exception(() => evt.EnsureSeatCanBeHeld("A1"));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureSeatCanBeHeld_ShouldNotChangeSeatStatus()
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        evt.EnsureSeatCanBeHeld("A1");
        evt.EnsureSeatCanBeHeld("A1");

        var seat = evt.Seats.Single(s => s.Name == "A1");
        Assert.Equal(ESeatStatus.Available, seat.Status);
    }

    [Fact]
    public void EnsureSeatCanBeHeld_BookedSeat_ShouldThrow()
    {
        var evt = new Event("Concert-01", new[] { "A1" });
        evt.Seats.Single(s => s.Name == "A1").Book();

        Assert.Throws<SeatNotAvailableException>(() => evt.EnsureSeatCanBeHeld("A1"));
    }

    [Fact]
    public void EnsureSeatCanBeHeld_SeatNotExist_ShouldThrow()
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        Assert.Throws<SeatNotFoundException>(() => evt.EnsureSeatCanBeHeld("Z9"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureSeatCanBeHeld_EmptySeatName_ShouldThrow(string seatName)
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        Assert.Throws<ArgumentException>(() => evt.EnsureSeatCanBeHeld(seatName));
    }

    [Fact]
    public void NewEvent_AllSeatsShouldBeAvailable()
    {
        var evt = new Event("Concert-01", new[] { "A1", "A2" });

        Assert.All(evt.Seats, seat => Assert.Equal(ESeatStatus.Available, seat.Status));
    }

    [Fact]
    public void NewEvent_DuplicateSeatNames_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Event("Concert-01", new[] { "A1", "A1" }));
    }

    [Fact]
    public void NewEvent_WhiteSpaceSeatName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Event("Concert-01", new[] { "   " }));
    }

    [Fact]
    public void NewEvent_NoSeats_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Event("Concert-01", Array.Empty<string>()));
    }

    [Fact]
    public void NewEvent_EmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Event("", new[] { "A1" }));
    }
}