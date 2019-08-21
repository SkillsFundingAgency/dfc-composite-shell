using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.UrlRewriter;
using FakeItEasy;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ContentProcessorServiceTests
    {
        private readonly ContentProcessorService contentProcessorService;

        public ContentProcessorServiceTests()
        {
            var fakeUrlRewriterService = A.Fake<IUrlRewriterService>();

            contentProcessorService = new ContentProcessorService(fakeUrlRewriterService);
        }

        [Fact]
        public void ProcessReturnsEmptyStringIfNoContent()
        {
            var result = contentProcessorService.Process(string.Empty, string.Empty, string.Empty);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void ProcessWhenThereIsContentCallsUrlRewriterServiceWithParametersSet()
        {
            const string someContent = "SomeContent";
            const string fakeBaseUrl = "FakeBaseUrl";
            const string fakeApplicationUrl = "FakeApplicationUrl";
            var fakeUrlRewriterService = A.Fake<IUrlRewriterService>();
            var validContentProcessorService = new ContentProcessorService(fakeUrlRewriterService);

            validContentProcessorService.Process(someContent, fakeBaseUrl, fakeApplicationUrl);

            A.CallTo(() => fakeUrlRewriterService.Rewrite(someContent, fakeBaseUrl, fakeApplicationUrl)).MustHaveHappenedOnceExactly();
        }
    }
}