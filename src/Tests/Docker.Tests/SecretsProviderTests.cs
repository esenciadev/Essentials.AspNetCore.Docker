using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Essentials.AspNetCore.Docker.Configuration;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Docker.Tests
{
    public class SecretsProviderTests
    {
        [Fact]
        public void WhenSecretsNotOptionalAndDirectoryIsNotPresentExceptionIsThrown()
        {
            var configSource = new SecretsConfigurationSource()
            {
                Optional = false
            };
            var cut = new SecretsConfigurationProvider(configSource);
            Assert.Throws<DirectoryNotFoundException>(() => cut.Load());
        }

        [Fact]
        public void WhenFilesPresentWithTwoUnderscoresInNamesTheyAreMappedCorrectly()
        {
            const string oneValue = "vdvdasdf";
            const string twoValue = "dasdfadsdf";
            
            var mockFileOne = GivenFileExists("some__secret__one", oneValue);
            var mockFileTwo = GivenFileExists("two", twoValue); 
            
            var configSource = GivenConfigurationSourceIsSetupToReturn(mockFileOne, mockFileTwo);
            
            var cut = new SecretsConfigurationProvider(configSource);

            cut.Load();

            AssertKeyWasLoaded(cut, "some:secret:one", oneValue);
            AssertKeyWasLoaded(cut, "two", twoValue);
        }

        [Fact]
        public void WhenDirectoriesPresentTheyAreIgnored()
        {
            const string oneValue = "vdvdasdf";
            const string twoValue = "dasdfadsdf";
            const string dirName = "zyx";
            
            var mockFileOne = GivenFileExists("some__secret__one", oneValue);
            var mockFileTwo = GivenFileExists("two", twoValue);
            var mockDir = GivenDirectoryEntryExists(dirName);
            
            var configSource = GivenConfigurationSourceIsSetupToReturn(mockFileOne, mockDir, mockFileTwo);
            
            var cut = new SecretsConfigurationProvider(configSource);

            cut.Load();

            AssertKeyWasLoaded(cut, "some:secret:one", oneValue);
            AssertKeyWasLoaded(cut, "two", twoValue);
            
            AssertKeyWasNotLoaded(cut, dirName);
        }

        [Fact]
        public void WhenFilesExistWithIgnorePrefixTheyAreIgnored()
        {
            const string oneValue = "vdvdasdf";
            const string twoValue = "dasdfadsdf";
            const string threeValue = "aaabbb";
            
            var mockFileOne = GivenFileExists("some__secret__one", oneValue);
            var mockFileTwo = GivenFileExists("two", twoValue);
            var mockFileThree = GivenFileExists("ignore.three", threeValue);
            
            var configSource = GivenConfigurationSourceIsSetupToReturn(mockFileOne, mockFileTwo, mockFileThree);
            
            var cut = new SecretsConfigurationProvider(configSource);

            cut.Load();

            // Confirm no variation on three was loaded
            AssertKeyWasNotLoaded(cut, "three");
            AssertKeyWasNotLoaded(cut, "ignore:three");
            AssertKeyWasNotLoaded(cut, "ignore.three");
            
            // Confirm the expected entries were loaded
            AssertKeyWasLoaded(cut,"some:secret:one", oneValue);
            AssertKeyWasLoaded(cut, "two", twoValue);
        }

        [Fact]
        public void WhenKeyHasSingleUnderscoreMixedItIsNotReplaced()
        {
            const string oneValue = "vdvdasdf";
            const string twoValue = "dasdfadsdf";
            
            var mockFileOne = GivenFileExists("some__secret__one_one", oneValue);
            var mockFileTwo = GivenFileExists("two__two", twoValue);

            var cut = GivenCutWithFiles(mockFileOne, mockFileTwo);

            cut.Load();
            
            AssertKeyWasLoaded(cut, "some:secret:one_one", oneValue);
            AssertKeyWasLoaded(cut, "two:two", twoValue);
        }
        
        #region Test helpers

        /// <summary>
        /// Assert that a given key exists, and has the specified value, in the provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <param name="expectedValue"></param>
        private static void AssertKeyWasLoaded(SecretsConfigurationProvider provider, 
                                               string key,
                                               string expectedValue)
        {
            var loaded = provider.TryGet(key, out var value);
            Assert.True(loaded);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Throw an exception if the given key exists in the provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        private static void AssertKeyWasNotLoaded(SecretsConfigurationProvider provider, string key)
        {
            Assert.False(provider.TryGet(key, out var _));
        }

        private static SecretsConfigurationProvider GivenCutWithFiles(params IFileInfo[] files)
        {
            var config = GivenConfigurationSourceIsSetupToReturn(files);
            return new SecretsConfigurationProvider(config);
        }

        /// <summary>
        /// Setup a <see cref="SecretsConfigurationSource"/> with sane test values, and a stub file provider that returns
        /// the given files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static SecretsConfigurationSource GivenConfigurationSourceIsSetupToReturn(params IFileInfo[] files)
        {    
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(x => x.GetDirectoryContents(It.IsAny<string>()))
                        .Returns(GivenDirectoryContentsExist(files));
            
            return new SecretsConfigurationSource()
            {
                Optional = false,
                FileProvider = fileProvider.Object
            };
        }

        /// <summary>
        /// Build an <see cref="IDirectoryContents"/> representing a directory which contains the given files.
        /// </summary>
        /// <param name="files">File infos that are 'in' the directory</param>
        /// <returns></returns>
        private static IDirectoryContents GivenDirectoryContentsExist(params IFileInfo[] files)
        {
            var mock = new Mock<IDirectoryContents>();
            mock.SetupGet(x => x.Exists).Returns(true);
            var enumerator = files.Cast<IFileInfo>().GetEnumerator();
            mock.Setup(x => x.GetEnumerator()).Returns(enumerator);
            return mock.Object;
        }

        /// <summary>
        /// Build an <see cref="IFileInfo"/> representing the given file, with content stream
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="contents">String content of the file, made available through CreateReadStream()</param>
        /// <returns>File object</returns>
        private static IFileInfo GivenFileExists(string fileName, string contents)
        {
            var mock = new Mock<IFileInfo>();
            mock.SetupGet(x => x.Name).Returns(fileName);
            var memoryStream = new MemoryStream();
            memoryStream.Write(Encoding.UTF8.GetBytes(contents));
            memoryStream.Seek(0, SeekOrigin.Begin);
            mock.Setup(x => x.CreateReadStream()).Returns(memoryStream);
            return mock.Object;
        }

        /// <summary>
        /// Build an <see cref="IFileInfo"/> representing the given directory name
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        private static IFileInfo GivenDirectoryEntryExists(string directoryName)
        {
            var mock = new Mock<IFileInfo>();
            mock.SetupGet(x => x.Name).Returns(directoryName);
            mock.SetupGet(x => x.IsDirectory).Returns(true);
            return mock.Object;
        }
        
        #endregion
    }
}