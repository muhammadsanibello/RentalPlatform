using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;
public class ListingImage
{
    public int Id { get; private set; }
    public string ImageUrl { get; private set; }
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public int ListingId { get; private set; }
    public Listing? Listing { get; private set; }

    // Used by EF Core when materializing existing records
    private ListingImage() { }

    private ListingImage(string imageUrl, bool isPrimary, int displayOrder, DateTime createdAt, int listingId)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new DomainException("Image URL cannot be empty.");
        }

        if (displayOrder <= 0)
        {
            throw new DomainException("Display order must be greater than zero.");
        }

        if (displayOrder > 5)
        {
            throw new DomainException("Display order cannot exceed five.");
        }

        if (createdAt == default)
        {
            throw new DomainException("Image creation time must be specified.");
        }

        if (listingId <= 0)
        {
            throw new DomainException("Listing Id must be greater than zero.");
        }

        ImageUrl = imageUrl.Trim();
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
        CreatedAt = createdAt;
        ListingId = listingId;
    }

    // Provide a controlled entry point for creating listing image
    public static ListingImage Create(string imageUrl, bool isPrimary, int displayOrder, DateTime createdAt, int listingId)
    {
        return new ListingImage(imageUrl, isPrimary, displayOrder, createdAt, listingId);
    }
}