namespace DiscountServer
{
    public interface IDiscountCodeGenerator
    {
        Task<List<string>> GenerateDiscountCodesAsync(int count);
    }

    public class DiscountCodeGenerator : IDiscountCodeGenerator
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _random = new Random();
        private readonly IFileDiscountStorage _fileStorage;

        public DiscountCodeGenerator(IFileDiscountStorage fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<List<string>> GenerateDiscountCodesAsync(int count)
        {
            if (count > 2000)
                throw new ArgumentException("Cannot generate more than 2000 codes in one request.");

            // Odczytujemy istniejące kody z pliku
            var existingCodes = new HashSet<string>(await _fileStorage.ReadDiscountCodesFromFileAsync());

            var newCodes = new HashSet<string>();

            while (newCodes.Count < count)
            {
                var code = GenerateRandomCode();
                if (!existingCodes.Contains(code) && !newCodes.Contains(code))
                {
                    newCodes.Add(code);
                }
            }

            return newCodes.ToList();
        }

        private string GenerateRandomCode()
        {
            var code = new char[8]; // Długość kodu rabatowego to 8 znaków
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = Chars[_random.Next(Chars.Length)];
            }
            return new string(code);
        }
    }
}