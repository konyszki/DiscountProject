using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountServer
{
    public interface IDiscountStorage
    {
        HashSet<string> FetchExistingCodes();
        void SaveNewCodes(IEnumerable<string> codes);
    }
    class FileDiscountStorage : IDiscountStorage
    {
        private readonly string _filePath;

        public FileDiscountStorage(string filePath)
        {
            _filePath = filePath;

            // Ensure the file exists
            if (!File.Exists(_filePath))
            {
                using (File.Create(_filePath)) { }
            }
        }

        public HashSet<string> FetchExistingCodes()
        {
            using (var reader = new StreamReader(_filePath))
            {
                var codes = new HashSet<string>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    codes.Add(line);
                }
                return codes;
            }
        }

        public void SaveNewCodes(IEnumerable<string> codes)
        {
            using (var writer = new StreamWriter(_filePath, append: true))
            {
                foreach (var code in codes)
                {
                    writer.WriteLine(code);
                }
            }
        }
    }
}
