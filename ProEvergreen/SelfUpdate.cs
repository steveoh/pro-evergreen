namespace ProEvergreen {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Octokit;
    using Semver;

    public class SelfUpdate {
        private readonly Assembly _assembly;
        private readonly GitHubClient _gitHubClient;
        private readonly string _repository;
        private readonly string _user;
        private bool _noRelease;
        private IReadOnlyList<Release> _releases;

        public SelfUpdate(string user, string repository) {
            _user = user;
            _repository = repository;
            _assembly = Assembly.GetCallingAssembly();
            _gitHubClient = new GitHubClient(new ProductHeaderValue("esri-pro-addin-self-update"));
        }

        public VersionInformation GetCurrentAddInVersion() {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var arcGisProLocation = Path.Combine(myDocs, "ArcGIS", "AddIns", "ArcGISPro");

            var attribute = (GuidAttribute) _assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var proAddinFolder = $"{{{attribute.Value}}}";

            var addinFolder = Path.Combine(arcGisProLocation, proAddinFolder);
            if (!Directory.Exists(addinFolder)) {
                return null;
            }

            // glob for *.esriAddinX
            var addins = Directory.GetFiles(addinFolder, "*.esriAddinX");

            if (!addins.Any()) {
                throw new ArgumentOutOfRangeException($"Could not find the addin in {addinFolder}");
            }

            if (addins.Length > 1) {
                throw new ArgumentOutOfRangeException($"Multiple esriAddinX files in {addinFolder}");
            }

            var addin = addins.FirstOrDefault();

            var entry = ZipFile.OpenRead(addin)
                               .Entries
                               .FirstOrDefault(x => x.Name.Equals("config.daml", StringComparison.InvariantCultureIgnoreCase));

            if (entry == null) {
                throw new ArgumentException("Could not find config.daml");
            }

            var text = new string(new StreamReader(entry.Open(), Encoding.UTF8).ReadToEnd().ToArray());

            var xmlDoc = XDocument.Parse(text);

            var ns = XNamespace.Get("http://schemas.esri.com/DADF/Registry");
            var addInInfo = xmlDoc.Root?.Element(ns + "AddInInfo");
            if (addInInfo == null) {
                throw new ArgumentException("could not find AddInInfo xml element");
            }

            var name = addInInfo.Element(ns + "Name");

            return new VersionInformation(name?.Value, addInInfo.Attributes().Single(x => x.Name == "version").Value,
                                          addInInfo.Attributes().Single(x => x.Name == "desktopVersion").Value);
        }

        public async Task<Release> GetLatestReleaseFromGithub() {
            if (_noRelease || _releases?.Count > 0) {
                return _releases[0];
            }

            _releases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            _releases = _releases.Where(x => x.Draft == false && x.Assets.Count > 0).ToList();

            if (_releases.Count >= 1) {
                return _releases[0];
            }

            _noRelease = true;

            return null;
        }

        public bool IsCurrent(string currentVersion, Release currentRelease) {
            // TODO: should we care about desktopVersion with incompatible pro version apis?
            // maybe add a .proversion file with the semver of the pro at build time
            var tagVersion = SemVersion.Parse(currentRelease.TagName.Replace("v", ""));

            if (currentVersion.Split('.').Length != 4) {
                return tagVersion <= currentVersion;
            }

            // we have assembly version
            var lastIndexOf = currentVersion.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
            currentVersion = currentVersion.Remove(lastIndexOf, currentVersion.Length - lastIndexOf);

            return tagVersion <= currentVersion;
        }

        public string Download(Release release) {
            var url = release.AssetsUrl;
            var assets = release.Assets;

            // figure out addin location

            return "assets";
        }

        public bool IsCompatible(string currentProVersion, string proVersionFromAssetsInRelease) {
            throw new NotImplementedException();
        }

        public void Update(string assets) {
            throw new NotImplementedException();
        }
    }

    public class VersionInformation {
        public VersionInformation(string addInName, string addInVersion, string targetProVersion) {
            AddInName = addInName;
            AddInVersion = addInVersion;
            TargetProVersion = targetProVersion;
        }

        public string AddInName { get; set; }

        public string AddInVersion { get; set; }

        public string TargetProVersion { get; set; }

        public override string ToString() {
            return $"Add In: {AddInName}\nVersion: {AddInVersion}\nArcGIS Pro Target Version: {TargetProVersion}";
        }
    }
}
