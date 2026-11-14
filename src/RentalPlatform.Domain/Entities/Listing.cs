using RentalPlatform.Domain.Enums;
using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;

public class Listing
{
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public RentDuration RentDuration { get; private set; }
    public ListingStatus Status { get; private set; } = ListingStatus.Draft;
    public DateTime CreatedAt { get; private set; }
    public bool Modified {  get; private set; }

    public int PropertyId { get; private set; }
    public Property? Property { get; private set; }

    public ICollection<ListingImage> Images { get; private set; } = new List<ListingImage>();

    public ICollection<ListingReview> Reviews { get; private set; } = new List<ListingReview>();

    public ICollection<RentalRequest> RentalRequests { get; private set; } = new List<RentalRequest>();

    public void SendForReview()
    {
        if ((Status is not ListingStatus.Draft) && (Status is not ListingStatus.Rejected))
        {
            throw new DomainException("Only draft or rejected listings can be sent for review.");
        }

        if (Status is ListingStatus.Rejected)
        {
            if (!Modified)
            {
                throw new DomainException("Rejected listings requires modification before resending for review.");
            }
        }

        Status = ListingStatus.PendingReview;

        if (Modified)
        {
            Modified = false;
        }
    }

    public void Publish()
    {
        if (Status is not ListingStatus.PendingReview)
        {
            throw new DomainException("Only pending-review listings can be published.");
        }

        Status = ListingStatus.Published;
    }

    public void Reject()
    {
        if (Status is not ListingStatus.PendingReview)
        {
            throw new DomainException("Only pending-review listings can be rejected.");
        }

        Status = ListingStatus.Rejected;
    }

    public void WithdrawTemporarily()
    {
        if (Status is not ListingStatus.Published)
        {
            throw new DomainException("Only published listings can be temporarily withdrawn.");
        }

        Status = ListingStatus.TemporarilyWithdrawn;
    }

    public void WithdrawPermanently()
    {
        if ((Status is not ListingStatus.Published) && (Status is not ListingStatus.TemporarilyWithdrawn))
        {
            throw new DomainException("Only published or temporarily-withdrawn listings can be permanently withdrawn.");
        }

        Status = ListingStatus.PermanentlyWithdrawn;
    }

    // Provide a controlled way of making the temporarily-withdrawn listings publicly discoverable again,
    // if no new changes are made.
    public void Republish()
    {
        if (Status is not ListingStatus.TemporarilyWithdrawn)
        {
            throw new DomainException("Only temporarily-withdrawn listings can be republished.");
        }

        if (Modified)
        {
            Status = ListingStatus.PendingReview;
            Modified = false;

            return;
        }

        Status = ListingStatus.Published;
    }

    public void ModifyListing(string? newTitle, string? newDescription, decimal? newPrice, RentDuration? newRentDuration)
    {
        if ((Status is not ListingStatus.Draft) && (Status is not ListingStatus.Rejected) && (Status is not ListingStatus.TemporarilyWithdrawn))
        {
            throw new DomainException("Only draft, rejected, or temporarily-withdrawn listings can be modified.");
        }

        if (newTitle is null && newDescription is null && newPrice is null && newRentDuration is null)
        {
            throw new DomainException("There is nothing to update.");
        }

        if (newTitle is not null)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                throw new DomainException("Title cannot be empty.");
            }
        }

        if (newDescription is not null)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
            {
                throw new DomainException("Description cannot be empty.");
            }
        }

        if (newPrice is not null)
        {
            if (newPrice <= 0)
            {
                throw new DomainException("Listing price must be greater than zero.");
            }
        }

        if (newRentDuration is not null)
        {
            if (!Enum.IsDefined((RentDuration)newRentDuration))
            {
                throw new DomainException("Invalid rent duration.");
            }
        }

        Title = newTitle is null ? Title : newTitle;
        Description = newDescription is null ? Description : newDescription;
        Price = newPrice is null ? Price : (decimal)newPrice;
        RentDuration = newRentDuration is null ? RentDuration : (RentDuration)newRentDuration;

        if (Status is ListingStatus.TemporarilyWithdrawn || Status is ListingStatus.Rejected)
        {
            Modified = true;
        }
    }

    // Used by EF Core when materializing existing records.
    private Listing() { }

    private Listing(string title, string? description, decimal price, RentDuration rentDuration, DateTime createdAt, int propertyId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title cannot be empty.");
        }

        if (price <= 0)
        {
            throw new DomainException("Listing price must be greater than zero.");
        }

        if (!Enum.IsDefined(rentDuration))
        {
            throw new DomainException("Invalid rent duration.");
        }

        if (createdAt == default)
        {
            throw new DomainException("Listing creation time must be specified.");
        }

        if (propertyId <= 0)
        {
            throw new DomainException("Property Id must be greater than zero.");
        }

        Title = title;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = price;
        RentDuration = rentDuration;
        CreatedAt = createdAt;
        PropertyId = propertyId;
    }

    // Provide a controlled entry point for creating a listing
    public static Listing Create(string title, string? description, decimal price, RentDuration rentDuration, DateTime createdAt, int propertyId)
    {
        return new Listing(title, description, price, rentDuration, createdAt, propertyId);
    }
}