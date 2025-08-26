namespace ProEvergreen
{
    internal class Program
    {
        private static void Main()
        {
            var updator = new Evergreen("steveoh", "pro-evergreen");
            var release = updator.GetLatestReleaseFromGithub().Result;

            var version = updator.GetCurrentAddInVersion();
            if (updator.IsCurrent(version.AddInVersion, release))
            {
                return;
            }

            var assets = updator.Update(release).Result;

            //            if (!updator.IsCompatible("currentProVersion", ".proversion from assets in release")) {
            //                throw new Exception("incompatible versions of pro");
            //            }
            //
            //            updator.Update(assets);
        }
    }
}
