using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class BatchingTests : TestBase
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(10)]
        [DataRow(11)]
        [DataRow(12)]
        [DataRow(13)]
        [DataRow(14)]
        [DataRow(15)]
        [DataRow(16)]
        [DataRow(17)]
        [DataRow(18)]
        [DataRow(19)]
        public void ToImageWithInteger(int? page)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page ?? 0}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (page == null)
                SavePng(outputStream, inputStream, options: new(Dpi: 40));
            else
                SavePng(outputStream, inputStream, page: page.Value, options: new(Dpi: 40));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        public void ToImagesWithSelectionOdd()
        {
            int[] selection = [1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = 0;

            foreach (var bitmap in ToImages(inputStream, selection, options: new(Dpi: 40)))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{selection[i]}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(10)]
        [DataRow(11)]
        [DataRow(12)]
        [DataRow(13)]
        [DataRow(14)]
        [DataRow(15)]
        [DataRow(16)]
        [DataRow(17)]
        [DataRow(18)]
        [DataRow(19)]
        public void ToImageWithIndex(int page)
        {
            var index = (Index)page;
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{index.GetOffset(20)}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            SavePng(outputStream, inputStream, index, options: new(Dpi: 40));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(10)]
        [DataRow(11)]
        [DataRow(12)]
        [DataRow(13)]
        [DataRow(14)]
        [DataRow(15)]
        [DataRow(16)]
        [DataRow(17)]
        [DataRow(18)]
        [DataRow(19)]
        public void ToImageWithIndexFromEnd(int page)
        {
            var index = new Index(page + 1, true);
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{index.GetOffset(20)}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            SavePng(outputStream, inputStream, index, options: new(Dpi: 40));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        public void ToImagesWithRangeAll()
        {
            var range = ..;

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = range.Start.Value;

            foreach (var bitmap in ToImages(inputStream, range, options: new(Dpi: 40)))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{i}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

        [TestMethod]
        public void ToImagesWithRangeSecondHalf()
        {
            var range = 10..;

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = range.Start.Value;

            foreach (var bitmap in ToImages(inputStream, range, options: new(Dpi: 40)))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{i}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

        [TestMethod]
        public void ToImagesWithSelectionEven()
        {
            int[] selection = [0, 2, 4, 6, 8, 10, 12, 14, 16, 18];

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = 0;

            foreach (var bitmap in ToImages(inputStream, selection, options: new(Dpi: 40)))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{selection[i]}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        public async Task ToImagesWithRangeAllAsync()
        {
            var range = ..;

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = range.Start.Value;

            await foreach (var bitmap in ToImagesAsync(inputStream, range, options: new(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{i}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

        [TestMethod]
        public async Task ToImagesWithRangeSecondHalfAsync()
        {
            var range = 10..;

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = range.Start.Value;

            await foreach (var bitmap in ToImagesAsync(inputStream, range, options: new(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{i}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

        [TestMethod]
        public async Task ToImagesWithSelectionEvenAsync()
        {
            int[] selection = [0, 2, 4, 6, 8, 10, 12, 14, 16, 18];

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = 0;

            await foreach (var bitmap in ToImagesAsync(inputStream, selection, options: new(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{selection[i]}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }

        [TestMethod]
        public async Task ToImagesWithSelectionOddAsync()
        {
            int[] selection = [1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "Wikimedia_Commons_web.pdf"));

            int i = 0;

            await foreach (var bitmap in ToImagesAsync(inputStream, selection, options: new(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{selection[i]}.png");
                using var outputStream = CreateOutputStream(expectedPath);
                bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedPath, outputStream);
                i++;
            }
        }
#endif
    }
}