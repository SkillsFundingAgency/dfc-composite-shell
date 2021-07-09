using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace DFC.Composite.Shell.Services.CookieParsers
{
    public class SetCookieParser : ISetCookieParser
    {
        private const string Path = "path";
        private const string HttpOnly = "httponly";
        private const string Secure = "secure";
        private const string SameSite = "samesite";
        private const string Strict = "strict";
        private const string Lax = "lax";
        private const string None = "none";
        private const string MaxAge = "max-age";

        public CookieSettings Parse(string value)
        {
            var result = new CookieSettings();

            if (!string.IsNullOrWhiteSpace(value))
            {
                var firstEqualPosition = value.IndexOf("=", StringComparison.OrdinalIgnoreCase);

                if (firstEqualPosition == -1)
                {
                    return result;
                }

                var cookieDataWithoutName = value[(firstEqualPosition + 1) ..];
                if (!string.IsNullOrWhiteSpace(cookieDataWithoutName))
                {
                    var dataSegments = cookieDataWithoutName.Split(';');

                    TrimSegments(dataSegments);
                    ParseKeyValue(result, value);
                    ParsePath(result.CookieOptions, dataSegments);
                    ParseSameSiteMode(result.CookieOptions, dataSegments);
                    ParseHttpOnly(result.CookieOptions, dataSegments);
                    ParseSecure(result.CookieOptions, dataSegments);
                    ParseMaxAge(result.CookieOptions, dataSegments);
                }

                return result;
            }

            return result;
        }

        private static string ParseValue(string[] segments, string segmentName)
        {
            var matchingSegment = segments
                .FirstOrDefault(segment => segment.StartsWith(segmentName, StringComparison.OrdinalIgnoreCase));

            if (matchingSegment == null)
            {
                return string.Empty;
            }

            var keyValuePair = matchingSegment.Split('=');
            return keyValuePair.Last();
        }

        private void TrimSegments(string[] dataSegments)
        {
            for (var i = 0; i < dataSegments.Length; i++)
            {
                var dataSegmentValue = dataSegments[i];
                if (!string.IsNullOrWhiteSpace(dataSegmentValue))
                {
                    dataSegments[i] = dataSegmentValue.Trim();
                }
            }
        }

        private void ParseKeyValue(CookieSettings cookieSettings, string value)
        {
            var segments = value.Split(';');

            if (!segments.Any())
            {
                return;
            }

            var keyValue = segments.First().Split("=");
            cookieSettings.Key = keyValue.FirstOrDefault();
            cookieSettings.Value = keyValue.LastOrDefault();
        }

        private void ParsePath(CookieOptions cookieOptions, string[] dataSegments)
        {
            cookieOptions.Path = ParseValue(dataSegments, Path);
        }

        private void ParseSameSiteMode(CookieOptions cookieOptions, string[] dataSegments)
        {
            var result = SameSiteMode.Lax;
            var ssm = ParseValue(dataSegments, SameSite);

            switch (ssm)
            {
                case Lax:
                    result = SameSiteMode.Lax;
                    break;
                case None:
                    result = SameSiteMode.None;
                    break;
                case Strict:
                    result = SameSiteMode.Strict;
                    break;
            }

            cookieOptions.SameSite = result;
        }

        private void ParseHttpOnly(CookieOptions cookieOptions, string[] dataSegments)
        {
            cookieOptions.HttpOnly = dataSegments.Any(segment => segment == HttpOnly);
        }

        private void ParseSecure(CookieOptions cookieOptions, string[] dataSegments)
        {
            cookieOptions.Secure = dataSegments.Any(segment => segment == Secure);
        }

        private void ParseMaxAge(CookieOptions cookieOptions, string[] dataSegments)
        {
            if (double.TryParse(ParseValue(dataSegments, MaxAge), out var maxAge))
            {
                cookieOptions.MaxAge = TimeSpan.FromSeconds(maxAge);
            }
        }
    }
}