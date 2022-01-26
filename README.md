# dfc-composite-shell


# Page registration examples

The file `PageRegistration\registration,json` contains a list of composite paths/regions to register automatically with the composite shell.

The full models for each are available in confluence:

* [Composite App Registration API](https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/2075001237/CUI+appRegistry+API)

The following are examples of how to register common items.

See the Composite App Registration API guide in confluence for full details.

## Registering a page with regions

To register a page, you can add the following to the json file:

```json
{
    "Path": "pathName",
    "TopNavigationText": "path Title",
    "TopNavigationOrder": 400,
    "Layout": 1,
    "OfflineHtml": "<div class=\"govuk-width-container\"><H2>Job Profile Service Unavailable</H2></div>",
    "PhaseBannerHtml": "<div class=\"govuk-phase-banner\">banner html</div>",
    "SitemapUrl": "<url to sitemap>",
    "RobotsUrl": "<url to robots.txt>",
    "Regions": [
        {
            "PageRegion": 1,
            "RegionEndpoint": "regionEndpoint"
        },
        {
            "PageRegion": 4,
            "RegionEndpoint": "regionEndpoint"
        }
    ]
}
```

## Registering an external path

To register an external path,  you can use the following example:

```json
{
    "Path": "externalPathName",
    "TopNavigationText": "Path Title",
    "TopNavigationOrder": 705,
    "Layout": 0,
    "OfflineHtml": "<div class=\"govuk-width-container\"><H2>External App is Unavailable</H2></div>",
    "ExternalUrl":  "external path url"
}
```