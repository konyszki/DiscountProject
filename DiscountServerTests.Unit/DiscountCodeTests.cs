using DiscountServer;
using NSubstitute;

namespace DiscountServerTests.Unit
{
    public class DiscountCodeTests
    {
        [Fact]
        public async Task GenerateDiscountCodesAsync_ShouldGenerateUniqueCodes()
        {
            // Arrange
            var fileStorage = Substitute.For<IFileDiscountStorage>();
            fileStorage.ReadDiscountCodesFromFileAsync().Returns(new List<string>()); // Brak istniej¹cych kodów

            var generator = new DiscountCodeGenerator(fileStorage);

            // Act
            var codes = await generator.GenerateDiscountCodesAsync(100);

            // Assert
            Assert.Equal(100, codes.Count);
            Assert.Equal(100, new HashSet<string>(codes).Count); // Unikalne kody
        }

        [Fact]
        public async Task GenerateDiscountCodesAsync_ShouldNotDuplicateExistingCodes()
        {
            // Arrange
            var existingCodes = new List<string> { "CODE123", "CODE456" };
            var fileStorage = Substitute.For<IFileDiscountStorage>();
            fileStorage.ReadDiscountCodesFromFileAsync().Returns(existingCodes);

            var generator = new DiscountCodeGenerator(fileStorage);

            // Act
            var codes = await generator.GenerateDiscountCodesAsync(100);

            // Assert
            Assert.DoesNotContain("CODE123", codes);
            Assert.DoesNotContain("CODE456", codes);
        }
    }
}