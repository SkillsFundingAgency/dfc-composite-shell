using DFC.Composite.Shell.Services.CookieParsers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class SetCookieParserTests
    {
        private SetCookieParser setCookieParser;

        public SetCookieParserTests()
        {
            setCookieParser = new SetCookieParser();
        }

        [Fact]
        public void CanParseAll()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; max-age=86400; path=/; samesite=lax; secure; httponly";

            var cookieSettings = setCookieParser.Parse(setCookieValue);

            Assert.Equal("AspNetCore.Session", cookieSettings.Key);
            Assert.Equal("CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT", cookieSettings.Value);
            Assert.Equal("/", cookieSettings.CookieOptions.Path);
            Assert.Equal(SameSiteMode.Lax, cookieSettings.CookieOptions.SameSite);
            Assert.True(cookieSettings.CookieOptions.HttpOnly);
            Assert.Equal(86400, cookieSettings.CookieOptions.MaxAge.Value.TotalSeconds);
            Assert.True(cookieSettings.CookieOptions.Secure);
        }

        [Fact]
        public void CanParseWhenPathIsNotForwardSlash()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=abc; samesite=strict; httponly";

            var cookieSettings = setCookieParser.Parse(setCookieValue);

            Assert.Equal("abc", cookieSettings.CookieOptions.Path);
        }

        [Fact]
        public void WhenHttpIsNotPresentShouldReturnFalseForHttpOnly()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=/; samesite=lax";

            var cookieSettings = setCookieParser.Parse(setCookieValue);

            Assert.False(cookieSettings.CookieOptions.HttpOnly);
        }

        [Fact]
        public void WhenSameSiteIsNotPresentShouldReturnLax()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=/";

            var cookieSettings = setCookieParser.Parse(setCookieValue);

            Assert.Equal(SameSiteMode.Lax, cookieSettings.CookieOptions.SameSite);
        }
    }
}
