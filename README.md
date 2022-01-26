# dfc-composite-shell

The Composite Shell uses a list of app registrations stored in the [App Registration API](https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/2075001237/CUI+appRegistry+API) micro-service. The data is used by the shell to determine which applications are available (menu options etc) and for each application, which page regions are to be populated from the apps.

# Configuring to run locally

The project contains *appsettings-template.json* files which contains Shell appsettings for the app. To use these files, copy them to *appsettings.json* within each project and edit and replace the configuration item values with values suitable for your environment.

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

## Assets

CSS, JS, images and fonts used in this site can found in the following repository [https://github.com/SkillsFundingAgency/dfc-digital-assets](https://github.com/SkillsFundingAgency/dfc-digital-assets)

## Built with

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Please refer to [https://github.com/SkillsFundingAgency/dfc-digital](https://github.com/SkillsFundingAgency/dfc-digital) for additional instructions on configuring individual components like Cosmos.
