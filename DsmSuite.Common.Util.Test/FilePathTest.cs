using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DsmSuite.Common.Util.Test
{
    [TestClass]
    public class FilePathTest
    {
        // todo Fix hardcoded temp paths.
        [TestMethod]
        public void WhenResolveFileIsCalledWithAnAbsoluteFilenameThenTheAbsoluteFileNameIsReturned()
        {
            string input = @"C:\temp\TestFileCopy.txt";
            string output = FilePath.ResolveFile(null, input);
            Assert.AreEqual(@"C:\temp\TestFileCopy.txt", output);
        }

        [TestMethod]
        public void WhenResolveFileIsCalledWithAnAbsolutePathAndFilenameThenTheResolvedAbsoluteFileNameIsReturned()
        {
            string path = @"C:\temp";
            string input = "TestFileCopy.txt";
            string output = FilePath.ResolveFile(path, input);
            Assert.AreEqual(@"C:\temp\TestFileCopy.txt", output);
        }

        [TestMethod]
        public void WhenResolveFileIsCalledWithAnAbsolutePathAndMultipleFilenamesThenTheResolvedAbsoluteFileNamesAreReturned()
        {
            string path = @"C:\temp";
            string[] input = { "TestFileCopy.txt", "TestFileCopyAgain.txt" };
            List<string> output = FilePath.ResolveFiles(path, input);
            Assert.AreEqual(@"C:\temp\TestFileCopy.txt", output[0]);
            Assert.AreEqual(@"C:\temp\TestFileCopyAgain.txt", output[1]);
        }

        [TestMethod]
        public void WhenResolveFileIsCalledWithAnAbsolutePathAndRelativeFilenameThenTheResolvedAbsoluteFileNameIsReturned()
        {
            string path = @"C:\temp\TestDir";
            string input = @"..\TestFileCopy.txt";
            string output = FilePath.ResolveFile(path, input);
            Assert.AreEqual(@"C:\temp\TestFileCopy.txt", output);
        }
    }
}
