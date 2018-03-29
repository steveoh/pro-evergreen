# pro-evergreen 

![pro evergreen](./proevergreen.png)

Never send an esri addin update to someone via email or a file share again!

Using github releases and release assets, you can automatically download and install new versions.

# Getting Started

### Prerequisites

1. You have a GitHub repository
1. The GitHub repository has semver releases with a `*.esriAddinX` file as an asset

### Nuget Installation

1. Search for and install `ProEvergreen` 

[https://www.nuget.org/packages/ProEvergreen/](https://www.nuget.org/packages/ProEvergreen/)

### Usage

```cs
var evergreen = new Evergreen("github username", "repository");
var evergreen = new Evergreen("steveoh", "pro-evergreen");
```

#### The API has four main uses currently. 

1. `evergreen.GetCurrentAddInVersion();` retuns a `VersionInformation` object that has the addin name, version, and the version of pro it was created with.
1. `evergreen.GetLatestReleaseFromGithub();` returns an OctoKit `Release` object with the information about the GitHub releases for the repository.
1. `evergreen.IsCurrent(addinVerion, OctoKit Release);` returns a `bool`. It is `true` if the semantic version of the addin is equal to what is available on GitHub. 
1. `evergreen.Update(OctoKit Release);` will download the current release, replacing and updating the current addin.


# Example

You can view an addin example in [this repository](https://github.com/steveoh/pro-evergreen/tree/master/ProEvergreen.AddIn)

This repository also has a [sample release](https://github.com/steveoh/pro-evergreen/releases)

# Publishing new releases
```
// create package
nuget pack ProEvergreen.csproj Configuration=Release

// publish
nuget push ProEvergreen.1.0.0.nupkg {apikey} -Source https://api.nuget.org/v3/index.json
```
