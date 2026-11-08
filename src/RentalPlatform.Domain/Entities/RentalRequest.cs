using RentalPlatform.Domain.Enums;
using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;
public class RentalRequest
{
    public int Id { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public RentalRequestStatus Status { get; private set; } = RentalRequestStatus.Pending;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public int TenantId { get; private set; }

    public int ListingId { get; private set; }
    public Listing? Listing { get; private set; }

    public Tenancy? Tenancy { get; private set; }

    private void EnsureCanBeResolved(DateTime resolvedAt, string operation)
    {
        if (Status is not RentalRequestStatus.Pending)
        {
            throw new DomainException($"Only pending rental requests can be {operation}.");
        }

        if (resolvedAt < SubmittedAt)
        {
            throw new DomainException("A rental request cannot be resolved before it was submitted.");
        }

        if (resolvedAt >= ExpiresAt)
        {
            throw new DomainException($"An expired rental request cannot be {operation}.");
        }
    }

    public void Approve(DateTime resolvedAt)
    {
        EnsureCanBeResolved(resolvedAt, "approved");

        Status = RentalRequestStatus.Approved;
        ResolvedAt = resolvedAt;
    }

    public void Reject(DateTime resolvedAt)
    {
        EnsureCanBeResolved(resolvedAt, "rejected");

        Status = RentalRequestStatus.Rejected;
        ResolvedAt = resolvedAt;
    }

    public void Withdraw(DateTime resolvedAt)
    {
        EnsureCanBeResolved(resolvedAt, "withdrawn");

        Status = RentalRequestStatus.Withdrawn;
        ResolvedAt = resolvedAt;
    }

    public void Expire(DateTime resolvedAt)
    {
        if (Status is not RentalRequestStatus.Pending)
        {
            throw new DomainException("Only pending rental requests can expire.");
        }

        if (resolvedAt < SubmittedAt)
        {
            throw new DomainException("A rental request cannot be resolved before it was submitted.");
        }

        if (resolvedAt < ExpiresAt)
        {
            throw new DomainException("The rental request has not yet reached its expiration time.");
        }

        Status = RentalRequestStatus.Expired;
        ResolvedAt = resolvedAt;
    }

    // Used by EF Core when materializing existing records.
    private RentalRequest() { }

    private RentalRequest(int tenantId, int listingId, DateTime submittedAt)
    {
        if (tenantId <= 0)
        {
            throw new DomainException("Tenant Id must be greater than zero.");
        }

        if (listingId <= 0)
        {
            throw new DomainException("Listing Id must be greater than zero.");
        }

        if (submittedAt == default)
        {
            throw new DomainException("The submission time must be specified.");
        }

        TenantId = tenantId;
        ListingId = listingId;
        SubmittedAt = submittedAt;
        ExpiresAt = SubmittedAt.AddDays(5);
    }

    // Provide the controlled entry point for creating a rental request.
    public static RentalRequest Create(int tenantId, int listingId, DateTime submittedAt)
    {
        return new RentalRequest(tenantId, listingId, submittedAt);
    }
}