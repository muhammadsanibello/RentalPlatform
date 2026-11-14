using RentalPlatform.Domain.Enums;
using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;
public class Tenancy
{
    public int RentalRequestId { get; private set; }
    public RentalRequest? RentalRequest { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal RentalAmount { get; private set; }
    public RentDuration Duration { get; private set; }
    public TenancyStatus Status { get; private set; } = TenancyStatus.Upcoming;
    public DateTime? TerminatedAt { get; private set; }

    public void Activate(DateOnly activationDate)
    {
        if (Status is not TenancyStatus.Upcoming)
        {
            throw new DomainException("Only Upcoming tenancies can be activated.");
        }

        if (activationDate < StartDate)
        {
            throw new DomainException("A tenancy cannot be activated before its start date.");
        }

        Status = TenancyStatus.Active;
    }

    public void Complete(DateOnly completionDate)
    {
        if (Status is not TenancyStatus.Active)
        {
            throw new DomainException("Only active tenancies can be completed.");
        }

        if (completionDate < EndDate)
        {
            throw new DomainException("A tenancy cannot be completed before its expiration date.");
        }

        Status = TenancyStatus.Completed;
    }

    public void Terminate(DateTime terminatedAt)
    {
        if ((Status is not TenancyStatus.Upcoming) && (Status is not TenancyStatus.Active))
        {
            throw new DomainException("Only upcoming or active tenancies can be terminated.");
        }

        if (terminatedAt == default)
        {
            throw new DomainException("Termination time must be specified.");
        }

        Status = TenancyStatus.Terminated;
        TerminatedAt = terminatedAt;
    }

    // Used by EF Core when materializing existing records.
    private Tenancy() { }

    private Tenancy(int rentalRequestId, DateOnly startDate, decimal rentalAmount, RentDuration duration)
    {
        if (rentalRequestId <= 0)
        {
            throw new DomainException("Rental request Id must be greater than zero.");
        }

        if (startDate == default)
        {
            throw new DomainException("Tenancy start date must be specified.");
        }

        if (rentalAmount <= 0)
        {
            throw new DomainException("Rental Amount must be greater than zero.");
        }

        if (!Enum.IsDefined(duration))
        {
            throw new DomainException("Invalid rent duration.");
        }

        RentalRequestId = rentalRequestId;
        StartDate = startDate;
        RentalAmount = rentalAmount;
        Duration = duration;
        EndDate = Duration is RentDuration.SixMonths ? StartDate.AddMonths(6) : StartDate.AddYears(1);
    }

    public static Tenancy Create(int rentalRequestId, DateOnly startDate, decimal rentalAmount, RentDuration duration)
    {
        return new Tenancy(rentalRequestId, startDate, rentalAmount, duration);
    }
}