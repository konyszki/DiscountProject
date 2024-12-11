using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountServer
{
    public interface IFileDiscountStorage
    {
        Task SaveDiscountCodesToFileAsync(List<string> codes);

        Task<List<string>> ReadDiscountCodesFromFileAsync();
    }

    internal class FileDiscountStorage : IFileDiscountStorage
    {
        private const string DiscountCodesFile = "discounts.txt";

        public async Task SaveDiscountCodesToFileAsync(List<string> codes)
        {
            // Zapisuje kody do pliku
            using (StreamWriter writer = new StreamWriter(DiscountCodesFile, append: true))
            {
                foreach (var code in codes)
                {
                    await writer.WriteLineAsync(code);
                }
            }
        }

        public async Task<List<string>> ReadDiscountCodesFromFileAsync()
        {
            var codes = new List<string>();

            if (File.Exists(DiscountCodesFile))
            {
                using (StreamReader reader = new StreamReader(DiscountCodesFile))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        codes.Add(line);
                    }
                }
            }

            return codes;
        }
    }
}