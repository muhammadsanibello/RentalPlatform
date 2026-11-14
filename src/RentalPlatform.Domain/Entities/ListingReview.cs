using RentalPlatform.Domain.Enums;
using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;
public class ListingReview
{
    public int Id { get; private set; }
    public ReviewDecision Decision { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ReviewedAt { get; private set; }

    public int ListingId { get; private set; }
    public Listing? Listing { get; private set; }

    public int ReviewedByAdminId { get; private set; }

    // Used by EF Core when materializing existing records
    private ListingReview() { }

    private ListingReview(ReviewDecision decision, string? comment, DateTime reviewedAt, int listingId, int reviewedByAdminId)
    {
        if (!Enum.IsDefined(decision))
        {
            throw new DomainException("Invalid decision.");
        }

        if (decision is ReviewDecision.Rejected && string.IsNullOrWhiteSpace(comment))
        {
            throw new DomainException("A rejection comment is required.");
        }

        if (reviewedAt == default)
        {
            throw new DomainException("Reviewed time must be specified.");
        }

        if (listingId <= 0)
        {
            throw new DomainException("Listing Id must be greater than zero.");
        }

        if (reviewedByAdminId <= 0)
        {
            throw new DomainException("Admin Id must be greater than zero.");
        }

        Decision = decision;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        ReviewedAt = reviewedAt;
        ListingId = listingId;
        ReviewedByAdminId = reviewedByAdminId;
    }

    public static ListingReview Create(ReviewDecision decision, string? comment, DateTime reviewedAt, int listingId, int reviewedByAdminId)
    {
        return new ListingReview(decision, comment, reviewedAt, listingId, reviewedByAdminId);
    }
}