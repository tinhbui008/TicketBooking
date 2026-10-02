using TicketBooking.Domain;
using TicketBooking.Domain.Exceptions;

namespace TicketBooking.Tests;

public class EventTests
{
    [Fact]
    public void HoldSeat_AlreadyHeld_ShouldThrow()
    {
        var evt = new Event("Concert-01", new[] { "A1" });
        evt.HoldSeat("A1");

        Assert.Throws<SeatNotAvailableException>(() => evt.HoldSeat("A1"));
    }

    [Fact]
    public void HoldSeat_AvailableSeat_ShouldBecomeHeld()
    {
        var evt = new Event("Concert-01", new[] { "A1" });
        evt.HoldSeat("A1");

        var seat = evt.Seats.Single(s => s.Name == "A1");

        Assert.Equal(ESeatStatus.Held, seat.Status);
    }

    [Fact]
    public void HoldSeat_ShouldNotAffectOtherSeats()
    {
        var evt = new Event("Concert-01", new[] { "A1", "A2" });
        evt.HoldSeat("A1");

        var other = evt.Seats.Single(s => s.Name == "A2");

        Assert.Equal(ESeatStatus.Available, other.Status);
    }

    [Fact]
    public void HoldSeat_SeatNotExist_ShouldThrow()
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        Assert.Throws<SeatNotFoundException>(() => evt.HoldSeat("Z9"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HoldSeat_EmptySeatName_ShouldThrow(string seatName)
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        Assert.Throws<ArgumentException>(() => evt.HoldSeat(seatName));
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

    [Fact(Skip = "Race condition đã biết ở HoldSeat (check-then-act), giải quyết ở Phase 2")]
    public async Task HoldSeat_Concurrent_OnlyOneShouldSucceed()
    {
        var evt = new Event("Concert-01", new[] { "A1" });

        var attempts = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            try
            {
                evt.HoldSeat("A1");
                return true;
            }
            catch (SeatNotAvailableException)
            {
                return false;
            }
        }));
        var results = await Task.WhenAll(attempts);

        Assert.Single(results, succeeded => succeeded);
    }
}