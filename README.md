# pro-evergreen 

![pro evergreen](./proevergreen.png)

Never send an esri addin update to someone via email or a file share again!

Using github releases and release assets, you can automatically download and install new versions.


# Publishing new releases
```
// create package
nuget pack ProEvergreen.csproj Configuration=Release

// publish
nuget push ProEvergreen.1.0.0.nupkg {apikey} -Source https://api.nuget.org/v3/index.json
```
