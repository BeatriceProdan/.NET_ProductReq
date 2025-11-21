using FluentAssertions;
using ProductsManagement.Features.Products.DTOs;
using Xunit;

namespace ProductsManagement.Test.Features.Products.DTOs;

public class ProductProfileDtoTests
{
    // DTO CREATION
    [Fact]
    public void Given_DefaultConstructor_When_Created_Then_DtoIsNotNull()
    {
        var dto = new ProductProfileDto();

        dto.Should().NotBeNull();
    }

    // PROPERTY ASSIGNMENT
    [Fact]
    public void Given_PropertyValues_When_Assigned_Then_ValuesAreStoredCorrectly()
    {
        var dto = new ProductProfileDto
        {
            Id = Guid.NewGuid(),
            Name = "Book",
            Brand = "Author House",
            SKU = "BK-100",
            CategoryDisplayName = "Books & Media",
            Price = 20m,
            FormattedPrice = "€20.00",
            ReleaseDate = new DateTime(2024, 1, 1),
            CreatedAt = new DateTime(2024, 1, 2),
            ImageUrl = "http://img.png",
            IsAvailable = true,
            StockQuantity = 10,
            ProductAge = "1 month",
            BrandInitials = "AH",
            AvailabilityStatus = "In Stock"
        };

        dto.Name.Should().Be("Book");
        dto.Brand.Should().Be("Author House");
        dto.SKU.Should().Be("BK-100");
        dto.CategoryDisplayName.Should().Be("Books & Media");
        dto.Price.Should().Be(20m);
        dto.FormattedPrice.Should().Be("€20.00");
        dto.ReleaseDate.Should().Be(new DateTime(2024, 1, 1));
        dto.CreatedAt.Should().Be(new DateTime(2024, 1, 2));
        dto.ImageUrl.Should().Be("http://img.png");
        dto.IsAvailable.Should().BeTrue();
        dto.StockQuantity.Should().Be(10);
        dto.ProductAge.Should().Be("1 month");
        dto.BrandInitials.Should().Be("AH");
        dto.AvailabilityStatus.Should().Be("In Stock");
    }

    // OPTIONAL FIELDS
    [Fact]
    public void Given_OptionalFields_When_Unset_Then_DefaultsAreEmptyOrNull()
    {
        var dto = new ProductProfileDto();

        dto.CategoryDisplayName.Should().BeEmpty();
        dto.BrandInitials.Should().BeEmpty();
        dto.FormattedPrice.Should().BeEmpty();
        dto.ProductAge.Should().BeEmpty();
        dto.AvailabilityStatus.Should().BeEmpty();
        dto.ImageUrl.Should().BeNull();
    }
}
