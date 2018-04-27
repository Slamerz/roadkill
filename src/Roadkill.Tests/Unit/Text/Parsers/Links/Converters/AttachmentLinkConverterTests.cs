using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Roadkill.Core.Configuration;
using Roadkill.Core.Text.Parsers.Links;
using Roadkill.Core.Text.Parsers.Links.Converters;

namespace Roadkill.Tests.Unit.Text.Parsers.Links.Converters
{
    public class AttachmentLinkConverterTests
    {
        private ApplicationSettings _applicationSettings;
        private Mock<IUrlHelper> _urlHelperMock;
        private AttachmentLinkConverter _converter;

        public AttachmentLinkConverterTests()
        {
            _applicationSettings = new ApplicationSettings();
            _urlHelperMock = new Mock<IUrlHelper>();
            _converter = new AttachmentLinkConverter(_applicationSettings, _urlHelperMock.Object);
        }

        [Fact]
        public void IsMatch_should_return_false_for_null_link()
        {
            // Arrange
            HtmlLinkTag htmlTag = null;

            // Act
            bool actualMatch = _converter.IsMatch(htmlTag);

            // Assert
            Assert.False(actualMatch);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("http://www.google.com", false)]
        [InlineData("internal-link", false)]
        [InlineData("attachment:/foo/bar.jpg", true)]
        [InlineData("~/foo/bar.jpg", true)]
        public void IsMatch_should_match_attachment_links(string href, bool expectedMatch)
        {
            // Arrange
            var htmlTag = new HtmlLinkTag(href, href, "text", "");

            // Act
            bool actualMatch = _converter.IsMatch(htmlTag);

            // Assert
            Assert.Equal(actualMatch, expectedMatch);
        }

        [Theory]
        [InlineData("http://www.google.com", "http://www.google.com", false)]
        [InlineData("internal-link", "internal-link", false)]
        [InlineData("attachment:foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        [InlineData("attachment:/foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        [InlineData("~/foo/bar.jpg", "/myattachments/foo/bar.jpg", true)]
        public void Convert_should_change_expected_urls_to_full_paths(string href, string expectedHref, bool calledUrlHelper)
        {
            // Arrange
            _urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns<string>(s => s);

            _applicationSettings.AttachmentsRoutePath = "myattachments";
            var originalTag = new HtmlLinkTag(href, href, "text", "");

            // Act
            var actualTag = _converter.Convert(originalTag);

            // Assert
            Assert.Equal(actualTag.OriginalHref, originalTag.OriginalHref);
            Assert.Equal(actualTag.Href, expectedHref);

            Times timesCalled = (calledUrlHelper) ? Times.Once() : Times.Never();
            _urlHelperMock.Verify(x => x.Content(It.IsAny<string>()), timesCalled);
        }
    }
}