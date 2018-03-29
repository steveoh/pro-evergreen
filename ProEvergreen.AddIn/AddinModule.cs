namespace ProEvergreen.AddIn {
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using ArcGIS.Desktop.Framework;
    using ArcGIS.Desktop.Framework.Contracts;

    internal class AddinModule : Module {
        private static AddinModule _this;

        /// <summary>
        ///     Retrieve the singleton instance to this module here
        /// </summary>
        public static AddinModule Current => _this ?? (_this = (AddinModule) FrameworkApplication.FindModule("ProEvergreen_AddIn_Module"));

        public Evergreen Evergreen { get; set; }
        public Octokit.Release Release { get; set; }
        #region Overrides

        /// <summary>
        ///     Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        public void ShowVersion() {
            Evergreen = new Evergreen("steveoh", "pro-evergreen");
            var versionInformation = Evergreen.GetCurrentAddInVersion();

            var version = new Notification {
                Message = versionInformation.ToString(),
                ImageUrl = "",
                Title = "Evergreen"
            };

            FrameworkApplication.AddNotification(version);
        }

        public async Task CheckForUpdate() {
            Release = await Evergreen.GetLatestReleaseFromGithub();
            var version = Evergreen.GetCurrentAddInVersion();

            var notification = new Notification
            {
                Message = "You are up to date.",
                ImageUrl = "",
                Title = "Evergreen: Version Check"
            };

            if (Evergreen.IsCurrent(version.AddInVersion, Release))
            {
                FrameworkApplication.AddNotification(notification);

                return;
            }

            notification.Message = $"Release version {Release.TagName} is available";

            FrameworkApplication.AddNotification(notification);
        }

        public async Task Update() {
            await Evergreen.Update(Release);

            var notification = new Notification
            {
                Message = "Restart to update.",
                ImageUrl = "",
                Title = "Evergreen: Upate Complete"
            };

            FrameworkApplication.AddNotification(notification);
        }
    }
}
