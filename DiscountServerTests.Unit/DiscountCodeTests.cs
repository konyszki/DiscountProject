using DiscountServer;
using NSubstitute;

namespace DiscountServerTests.Unit
{
    public class DiscountCodeTests
    {
        [Fact]
        public void GenerateUniqueDiscountCodes_ShouldReturnCorrectNumberOfCodes()
        {
            // Arrange
            var discountStorage = Substitute.For<IDiscountStorage>();
            discountStorage.FetchExistingCodes().Returns(new HashSet<string> { "ABC1234", "DEF5678" });

            var server = new WebSocketServer(discountStorage);

            // Act
            var codes = server.GenerateUniqueDiscountCodes(5, new HashSet<string> { "ABC1234", "DEF5678" }).ToList();

            // Assert
            Assert.NotNull(codes);
            Assert.Equal(5, codes.Count);
            Assert.DoesNotContain("ABC1234", codes);
            Assert.DoesNotContain("DEF5678", codes);
        }

        [Fact]
        public void FetchOrGenerateDiscountCodes_ShouldReturnExistingAndNewCodes()
        {
            // Arrange
            var existingCodes = new HashSet<string> { "ABC1234", "DEF5678" };
            var discountStorage = Substitute.For<IDiscountStorage>();
            discountStorage.FetchExistingCodes().Returns(existingCodes);

            var server = new WebSocketServer(discountStorage);

            // Act
            var codes = server.FetchOrGenerateDiscountCodes(3).ToList();

            // Assert
            Assert.NotNull(codes);
            Assert.True(codes.Count >= 3);
        }
    }
}