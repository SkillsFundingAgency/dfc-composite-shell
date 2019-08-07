using DFC.Composite.Shell.Services.CookieParsers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class CookieParserTests
    {
        private CookieParser cookieParser;

        public CookieParserTests()
        {
            cookieParser = new CookieParser();
        }

        [Fact]
        public void CanParseAll()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=/; samesite=lax; httponly";

            var cookieOptions = cookieParser.Parse(setCookieValue);
            Assert.Equal("/", cookieOptions.Path);
            Assert.Equal(SameSiteMode.Lax, cookieOptions.SameSite);
            Assert.True(cookieOptions.HttpOnly);
        }

        [Fact]
        public void CanParseWhenPathIsNotForwardSlash()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=abc; samesite=strict; httponly";

            var cookieOptions = cookieParser.Parse(setCookieValue);
            Assert.Equal("abc", cookieOptions.Path);
            Assert.Equal(SameSiteMode.Strict, cookieOptions.SameSite);
            Assert.True(cookieOptions.HttpOnly);
        }

        [Fact]
        public void WhenHttpIsNotPresentShouldReturnFalseForHttpOnly()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=/; samesite=lax";

            var cookieOptions = cookieParser.Parse(setCookieValue);
            Assert.False(cookieOptions.HttpOnly);
        }

        [Fact]
        public void WhenSameSiteIsNotPresentShouldReturnLax()
        {
            var setCookieValue = "AspNetCore.Session=CfDJ8FZO5gtnlm1PljkZVn4PRhHdTy%2BGI4kAZdel9FFlyqlGlmUqangpRTzJpWMJ6Sz6QL9ESgnO%2FunS%2B0pT7DGdMcijgo8xIcYGk4ZmKCYD%2Fp1RzXEO6yzslVp8yYu43d2%2FXqKA1U93jjpXFSLgV5eYUlSKK4PyZ0MAvo5xEboZdRsT; path=/";

            var cookieOptions = cookieParser.Parse(setCookieValue);
            Assert.Equal(SameSiteMode.Lax, cookieOptions.SameSite);
        }
    }
}
