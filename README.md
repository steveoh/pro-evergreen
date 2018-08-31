# pro-evergreen 
[![NuGet version](https://badge.fury.io/nu/ProEvergreen.svg)](https://www.nuget.org/packages/ProEvergreen/)

![pro evergreen](./proevergreen.png)


Never send an esri addin update to someone via email or a file share again!

Using github releases and release assets, you can automatically download and install new versions.

# What? Why?!

Esri is **not** in the business of creating an addin registry like npm has for node or pypi has for python. This class library makes it simple for you to add the functionality to auto update pro addins for your users. When you add pro evergreen as a reference to your existing addin, you get access to methods that will check the version of the addin running with the version of the addin that you have made available on GitHub. You have access to methods to download and update the addin so when your users restart pro, the new code will be live.

You are in charge of the update flow. You can be passive with a popup that suggests your user to update the addin or you can make the addin update as soon as it knows there is a new version. It is up to you!

# Getting Started

### Prerequisites

1. You have a GitHub repository
1. The GitHub repository has semver releases with a `*.esriAddinX` file as an asset

### Nuget Installation

- **NuGet package manager**: Search for `ProEvergreen` [https://www.nuget.org/packages/ProEvergreen/](https://www.nuget.org/packages/ProEvergreen/)
- **Powershell**: `Install-Package ProEvergreen

### Usage

```cs
var evergreen = new Evergreen("github username", "repository");
var evergreen = new Evergreen("steveoh", "pro-evergreen");
```

#### The API has four main uses currently. 

1. `evergreen.GetCurrentAddInVersion();` retuns a `VersionInformation` object that has the addin name, version, and the version of pro it was created with. Will throw a `ArgumentOutOfRangeException` if it can't find the file or an `ArgumentException` if it can't find the config.daml.
1. `evergreen.GetLatestReleaseFromGithub();` returns an OctoKit `Release` object with the information about the GitHub releases for the repository. Will throw a `ArgumentNullException` if either parameters are empty.
1. `evergreen.IsCurrent(addinVerion, OctoKit Release);` returns a `bool`. It is `true` if the semantic version of the addin is equal to what is available on GitHub. 
1. `evergreen.Update(OctoKit Release);` will download the current release, replacing and updating the current addin. Will throw an `ArgumentNullException` if the release is null.


# Example

You can view an addin example in [this repository](https://github.com/steveoh/pro-evergreen/tree/master/ProEvergreen.AddIn).

This repository also has a [sample release](https://github.com/steveoh/pro-evergreen/releases).

# Projects using Evergreen

1. https://github.com/agrc/uic-addin
2. [roemhildtg/arcgis-pro-addins](https://github.com/roemhildtg/arcgis-pro-addins) - streetview and selection tools addins

_send a pr to add your project!_

# Publishing new releases
1. create nuspec and edit output
   - `nuget spec ProEvergreen.csproj`
1. edit nuspec release notes
1. release build
1. create package
   - `nuget pack ProEvergreen.csproj -Properties Configuration=Release`
1. publish
   - `nuget push ProEvergreen.{M.m.p}.nupkg {apikey} -Source https://api.nuget.org/v3/index.json`
