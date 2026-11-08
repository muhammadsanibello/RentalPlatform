using RentalPlatform.Domain.Enums;
using RentalPlatform.Domain.Exceptions;

namespace RentalPlatform.Domain.Entities;
public class Property
{
    public int Id { get; private set; }
    public string? Name { get; private set; }
    public string State { get; private set; }
    public string City { get; private set; }
    public string StreetAddress { get; private set; }
    public string LGA { get; private set; }
    public PropertyType PropertyType { get; private set; }
    public PropertyAvailabilityStatus AvailabilityStatus { get; private set; } = PropertyAvailabilityStatus.Available;
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
        
    public int LandlordId { get; private set; }

    public int? ParentPropertyId { get; private set; }
    public Property? ParentProperty { get; private set; }
        
    public ICollection<Property> ChildProperties { get; private set; } = new List<Property>();

    public ICollection<Listing> Listings { get; private set; } = new List<Listing>();

    public void Hold()
    {
        if (AvailabilityStatus is not PropertyAvailabilityStatus.Available)
        {
            throw new DomainException("Only available properties can be held.");
        }

        AvailabilityStatus = PropertyAvailabilityStatus.Held;
    }

    public void Occupy()
    {
        if (AvailabilityStatus is not PropertyAvailabilityStatus.Held)
        {
            throw new DomainException("Only held properties can be occupied.");
        }

        AvailabilityStatus = PropertyAvailabilityStatus.Occupied;
    }

    public void MarkUnavailable()
    {
        if (AvailabilityStatus is PropertyAvailabilityStatus.Unavailable)
        {
            throw new DomainException("The property is already unavailable.");
        }

        if (AvailabilityStatus is PropertyAvailabilityStatus.Held)
        {
            throw new DomainException("The held property cannot be marked as unavailable.");
        }


        AvailabilityStatus = PropertyAvailabilityStatus.Unavailable;
    }

    public void MarkAvailable()
    {
        if (AvailabilityStatus is PropertyAvailabilityStatus.Available)
        {
            throw new DomainException("The property is already available.");
        }

        if (AvailabilityStatus is PropertyAvailabilityStatus.Occupied)
        {
            throw new DomainException("An occupied property cannot be marked as available.");
        }

        AvailabilityStatus = PropertyAvailabilityStatus.Available;
    }

    public void SoftDelete(DateTime deletedAt)
    {
        if (DeletedAt is not null)
        {
            throw new DomainException("The property has already been deleted.");
        }

        if (deletedAt < CreatedAt)
        {
            throw new DomainException("A property cannot be deleted before it was created.");
        }

        DeletedAt = deletedAt;
    }

    // Used by EF Core when materializing existing records.
    private Property() { }

    private Property(string? name, string state, string city, string streetAddress, string LGA, PropertyType propertyType, DateTime createdAt, int landlordId, int? parentPropertyId)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new DomainException("The state field must have a value.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException("The city field must have a value.");
        }

        if (string.IsNullOrWhiteSpace(streetAddress))
        {
            throw new DomainException("The street address field must have a value.");
        }

        if (string.IsNullOrWhiteSpace(LGA))
        {
            throw new DomainException("The LGA field must have a value.");
        }

        if (!Enum.IsDefined(propertyType))
        {
            throw new DomainException("Invalid property type.");
        }

        if (createdAt == default)
        {
            throw new DomainException("The property creation time must be specified.");
        }

        if (landlordId <= 0)
        {
            throw new DomainException("Landlord Id must be greater than zero.");
        }

        if (parentPropertyId is not null)
        {
            if (parentPropertyId <= 0)
            {
                throw new DomainException("Parent property Id must be greater than zero.");
            }
        }

        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        State = state;
        City = city;
        StreetAddress = streetAddress;
        this.LGA = LGA;
        PropertyType = propertyType;
        CreatedAt = createdAt;
        LandlordId = landlordId;
        ParentPropertyId = parentPropertyId;
    }

    // Provide the controlled entry point for creating a property.
    public static Property Create(string? name, string state, string city, string streetAddress, string LGA, PropertyType propertyType, DateTime createdAt, int landlordId, int? parentPropertyId)
    {
        return new Property(name, state, city, streetAddress, LGA, propertyType, createdAt, landlordId, parentPropertyId);
    }
}