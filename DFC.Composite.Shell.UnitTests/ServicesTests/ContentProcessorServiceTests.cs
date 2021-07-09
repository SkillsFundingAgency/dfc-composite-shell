using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.UrlRewriter;
using FakeItEasy;
using System;
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
            var result = contentProcessorService.Process(string.Empty, null, null);
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void ProcessWhenThereIsContentCallsUrlRewriterServiceWithParametersSet()
        {
            const string someContent = "SomeContent";
            var fakeBaseUrl = new Uri("http://FakeBaseUrl");
            var fakeApplicationUrl = new Uri("http://FakeApplicationUrl");
            var fakeUrlRewriterService = A.Fake<IUrlRewriterService>();
            var validContentProcessorService = new ContentProcessorService(fakeUrlRewriterService);

            validContentProcessorService.Process(someContent, fakeBaseUrl, fakeApplicationUrl);

            A.CallTo(() => fakeUrlRewriterService.RewriteAttributes(someContent, fakeBaseUrl, fakeApplicationUrl)).MustHaveHappenedOnceExactly();
        }
    }
}
