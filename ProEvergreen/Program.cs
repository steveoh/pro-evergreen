namespace ProEvergreen {
    using System;

    internal class Program {
        private static void Main() {
            var updator = new SelfUpdate("steveoh", "pro-evergreen");
            var release = updator.GetLatestReleaseFromGithub().Result;

            var version = updator.GetCurrentAddInVersion();
            if (updator.IsCurrent(version.AddInVersion, release)) {
                return;
            }

            var assets = updator.Download(release);

            if (!updator.IsCompatible("currentProVersion", ".proversion from assets in release")) {
                throw new Exception("incompatible versions of pro");
            }

            updator.Update(assets);
        }
    }
}
