using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace DFC.Composite.Shell.Services.CookieParsers
{
    public class CookieParser : ICookieParser
    {
        private const string Path = "path";
        private const string HttpOnly = "httponly";
        private const string SameSite = "samesite";
        private const string Strict = "strict";
        private const string Lax = "lax";
        private const string None = "none";

        public CookieOptions Parse(string value)
        {
            var result = new CookieOptions();

            if (!string.IsNullOrWhiteSpace(value))
            {
                var firstEqualPosition = value.IndexOf("=", StringComparison.OrdinalIgnoreCase);
                if (firstEqualPosition != -1)
                {
                    var cookieDataWithoutName = value.Substring(firstEqualPosition + 1);
                    if (!string.IsNullOrWhiteSpace(cookieDataWithoutName))
                    {
                        var dataSegments = cookieDataWithoutName.Split(';');

                        //Ensure all values are trimmed before comparing
                        for (var i = 0; i < dataSegments.Length; i++)
                        {
                            var dataSegmentValue = dataSegments[i];
                            if (!string.IsNullOrEmpty(dataSegmentValue))
                            {
                                dataSegments[i] = dataSegmentValue.Trim();
                            }
                        }

                        //Get the path value
                        result.Path = ParseValue(dataSegments, Path);

                        //Get the samesitemode value
                        var ssm = ParseValue(dataSegments, SameSite);
                        switch (ssm)
                        {
                            case Lax:
                                result.SameSite = SameSiteMode.Lax;
                                break;
                            case None:
                                result.SameSite = SameSiteMode.None;
                                break;
                            case Strict:
                                result.SameSite = SameSiteMode.Strict;
                                break;
                            default:
                                result.SameSite = SameSiteMode.Lax;
                                break;
                        }

                        //Determine if its httponly
                        result.HttpOnly = dataSegments.Any(x => x.StartsWith(HttpOnly, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return result;
        }

        private string ParseValue(string[] segments, string segmentName)
        {
            var result = string.Empty;
            var matchingSegment = segments.FirstOrDefault(x => x.StartsWith(segmentName, StringComparison.OrdinalIgnoreCase));
            if (matchingSegment != null)
            {
                var keyValuePair = matchingSegment.Split('=');
                result = keyValuePair.Last();
            }

            return result;
        }
    }
}
