using ProEvergreen.models;

namespace ProEvergreen {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Octokit;
    using Semver;
    using FileMode = System.IO.FileMode;

    public class Evergreen {
        private readonly Assembly _assembly;
        private readonly HttpClient _client;
        private readonly GitHubClient _gitHubClient;
        private readonly string _repository;
        private readonly string _user;
        private string _addinFolder;
        private readonly Dictionary<bool, ReleaseTicket> _tickets = new Dictionary<bool, ReleaseTicket>(2)
        {
            { true, new ReleaseTicket() },
            { false, new ReleaseTicket() }
        };

        public Evergreen(string user, string repository, GitHubOptions options=null) {
            _user = user;
            _repository = repository;
            _assembly = Assembly.GetCallingAssembly();

            if (options?.GitHubEnterpriseUri != null) {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("esri-pro-addin-self-update"), options.GitHubEnterpriseUri);
            } else {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("esri-pro-addin-self-update"));
            }

            if (options?.Credentials != null) {
                _gitHubClient.Credentials = options.Credentials;
            }

            _client = new HttpClient {
                Timeout = TimeSpan.FromMinutes(1)
            };
        }
        /// <summary>
        /// Returns the version information from the config.daml from inside the default addin folder
        /// </summary>
        /// <returns>VersionInformation has the addin name, it's version and the pro version it was compiled for</returns>
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

            XDocument doc;
            using (var zip = ZipFile.OpenRead(addin)) {
                var entry = zip.Entries
                               .FirstOrDefault(x => x.Name.Equals("config.daml", StringComparison.InvariantCultureIgnoreCase));

                if (entry == null) {
                    throw new ArgumentException("Could not find config.daml");
                }

                using (var stream = new StreamReader(entry.Open(), Encoding.UTF8)) {
                    var text = new string(stream.ReadToEnd().ToCharArray());

                    doc = XDocument.Parse(text);
                }
            }

            var ns = XNamespace.Get("http://schemas.esri.com/DADF/Registry");
            var addInInfo = doc.Root?.Element(ns + "AddInInfo");
            if (addInInfo == null) {
                throw new ArgumentException("could not find AddInInfo xml element");
            }

            var name = addInInfo.Element(ns + "Name");

            _addinFolder = addinFolder;

            return new VersionInformation(name?.Value, addInInfo.Attributes().Single(x => x.Name == "version").Value,
                                          addInInfo.Attributes().Single(x => x.Name == "desktopVersion").Value);
        }

        /// <summary>
        /// Get the latest releases from GitHub. Use pre releases if you want to allow your users to be on the beta channel.
        /// </summary>
        /// <param name="includePrerelease">true if you want "beta" releases to be included.</param>
        /// <returns>http://octokitnet.readthedocs.io/en/latest/releases/</returns>
        public async Task<Release> GetLatestReleaseFromGithub(bool includePrerelease=false) {
            if (_tickets[includePrerelease].Punched)
            {
                return _tickets[includePrerelease].Releases.FirstOrDefault();
            }

            _tickets[includePrerelease].Releases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            _tickets[includePrerelease].Releases = _tickets[includePrerelease].Releases.Where(x => x.Prerelease == includePrerelease && x.Draft == false && x.Assets.Count > 0).ToList();

            _tickets[includePrerelease].Punched = true;

            return _tickets[includePrerelease].Releases.FirstOrDefault();
        }

        /// <summary>
        /// Returns true if the current version matches the release version. This uses the standard semantic versioning model.
        /// </summary>
        /// <param name="currentVersion">The addin version</param>
        /// <param name="currentRelease">The github release version object</param>
        /// <returns></returns>
        public bool IsCurrent(string currentVersion, Release currentRelease) {
            // TODO: should we care about desktopVersion with incompatible pro version apis?
            // maybe add a .proversion file with the semver of the pro at build time

            if (string.IsNullOrEmpty(currentVersion))
            {
                throw new ArgumentNullException(nameof(currentVersion), "The pro addin version could not be found. Please submit an issue on github.");
            }

            if (currentRelease == null)
            {
                throw new ArgumentNullException(nameof(currentRelease), "The GitHub release is null. A release will be ignored if it does not contains a .esriAddinX " +
                                                                        "release asset or if it is a pre-release and includPrereleases is false.");
            }

            var tagVersion = SemVersion.Parse(currentRelease.TagName.Replace("v", ""));

            if (currentVersion.Split('.').Length != 4) {
                return tagVersion <= currentVersion;
            }

            return tagVersion <= currentVersion;
        }

        /// <summary>
        /// Download the esriAddinX from the provided github release and install it in the addin folder. A restart will be required to 
        /// see the new changes.
        /// </summary>
        /// <param name="release">The OctoKit.Release that contains the new addin</param>
        /// <returns></returns>
        public async Task<string> Update(Release release) {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release), "The GitHub release is null. A release will be ignored if it does not contains a .esriAddinX " +
                                                                        "release asset or if it is a pre-release and includPrereleases is false.");
            }

            var addinAsset = release.Assets.Single(x => x.Name.EndsWith(".esriAddinX", StringComparison.InvariantCultureIgnoreCase));
            var newAddinDownloadLocation = Path.Combine(_addinFolder, addinAsset.Name);


            using (var request = new HttpRequestMessage(HttpMethod.Get, addinAsset.BrowserDownloadUrl))
            using (var response = await _client.SendAsync(request))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(newAddinDownloadLocation, FileMode.Create)) {
                await stream.CopyToAsync(fileStream);
            }

            return newAddinDownloadLocation;
        }

        public bool IsCompatible(string currentProVersion, string proVersionFromAssetsInRelease) {
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
            return $"Add In: {AddInName}\nVersion: {AddInVersion}\nArcGIS Pro Version: {TargetProVersion}";
        }
    }
}
