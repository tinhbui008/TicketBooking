using TicketBooking.Domain;

namespace TicketBooking.Tests;

public class BookingTests
{
    [Fact]
    public void NewBooking_ShouldBe_SeatHeld()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        Assert.Equal(EBookingStatus.SeatHeld, booking.Status);
    }

    [Fact]
    public void NewBooking_EmptyEventId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Booking(Guid.Empty, "A1"));
    }

    [Fact]
    public void NewBooking_EmptySeatName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Booking(Guid.NewGuid(), ""));
    }

    [Fact]
    public void Confirm_FromSeatHeld_ShouldBeConfirmed()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();

        Assert.Equal(EBookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();

        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Confirm_WhenExpired_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Expire();

        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Expire_FromSeatHeld_ShouldBeExpired()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Expire();

        Assert.Equal(EBookingStatus.Expired, booking.Status);
    }

    [Fact]
    public void Expire_WhenConfirmed_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();

        Assert.Throws<InvalidOperationException>(() => booking.Expire());
    }

    [Fact]
    public void Expire_WhenAlreadyExpired_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Expire();

        Assert.Throws<InvalidOperationException>(() => booking.Expire());
    }
}