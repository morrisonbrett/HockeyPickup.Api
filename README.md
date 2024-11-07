[![Twitter](https://img.shields.io/twitter/follow/morrisonbrett?style=social)](https://x.com/morrisonbrett)

[![HockeyPickup.Api](https://github.com/morrisonbrett/HockeyPickup.Api/actions/workflows/master_hockeypickupapi.yml/badge.svg)](https://github.com/morrisonbrett/HockeyPickup.Api/actions/workflows/master_hockeypickupapi.yml)
[![Coverage Status](https://coveralls.io/repos/github/morrisonbrett/HockeyPickup.Api/badge.svg)](https://coveralls.io/github/morrisonbrett/HockeyPickup.Api)

# 🏒 HockeyPickup.Api

## 🌈 Overview

HockeyPickup.Api is the core backend for [HockeyPickup](https://hockeypickup.com).

The main technology stack platform is [.NET Core](https://dotnet.microsoft.com/) 8.0.

## 🛠 Prerequisites

* Install Visual Studio 2022 (preview) or later, or Visual Studio Code. Ensure that `$ dotnet --version` is at least 8.0.

## ⌨️ Install, Build, and Serve the Site

Create a new file at the root of the HockeyPickup.Api project named `appsettings.json` with the following contents:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.AspNetCore.Mvc.ApiExplorer": "Debug"
    },
    "Console": {
      "IncludeScopes": true,
      "LogLevel": {
        "Default": "Debug",
        "System": "Information",
        "Microsoft": "Information",
        "Microsoft.AspNetCore.Mvc.ApiExplorer": "Debug"
      }
    }
  },
  "AllowedHosts": "*",
  "WEBSITE_CONTENTAZUREFILESCOMPATIBILITYOVERRIDE": 1,
  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
  }
}
```

### Install the packages

```bash
$ dotnet restore
$ dotnet tool restore
```
Open HockeyPickup.Api.sln solution in Visual Studio, and build the solution.

You'll see output in the console showing the various local URL access points.

Swagger root [`https://localhost:7042/swagger/index.html`](https://localhost:7042/swagger/index.html)

GraphQL root [`https://localhost:7042/api/graphql`](https://localhost:7042/api/graphql)

## 🧪 Unit Testing

Unit testing and code coverage are setup and **must** be maintained. To run the tests and generate a coverage report, run the Powershell script from the command line.

```bash
$ powershell ./scripts/RunTests.ps1
```

This generates a coverage report in `HockeyPickup.Api.Tests/coverage-html`. Open `index.html` to view the report.

## 📮 Making requests via Postman

[Postman](https://www.postman.com/) is a useful tool for testing Apis.

## 🎁 Versioning

HockeyPickup.Api uses [sementic versioning](https://semver.org/), starting with 1.0.0.

The patch (last segment of the 3 segments) is auto-incremented via a GitHub action when a pull request is merged to master. The GitHub action is configured in [.github/workflows/hockeypickup-api-version.yml](.github/workflows/hockeypickup-api-version.yml). To update the major or minor version, follow the instructions specified in the [bumping section of the action](https://github.com/anothrNick/github-tag-action#bumping) - use #major or #minor in the commit message to auto-increment the version.

## ❤️ Contributing

We welcome useful contributions. Please read our [contributing guidelines](CONTRIBUTING.md) before submitting a pull request.

## 📜 License

HockeyPickup.Api is licensed under the MIT license.

[![License](https://img.shields.io/github/license/morrisonbrett/HockeyPickup.Api)]((https://github.com/morrisonbrett/HockeyPickup.Api/master/LICENSE))

[hockeypickup.com](https://hockeypickup.com)
<!---
Icons used from: https://emojipedia.org/
--->