namespace ProEvergreen.AddIn {
    using ArcGIS.Desktop.Framework;
    using ArcGIS.Desktop.Framework.Contracts;

    internal class AddinModule : Module {
        private static AddinModule _this;

        /// <summary>
        ///     Retrieve the singleton instance to this module here
        /// </summary>
        public static AddinModule Current => _this ?? (_this = (AddinModule) FrameworkApplication.FindModule("ProEvergreen_AddIn_Module"));

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
            var updator = new SelfUpdate("steveoh", "pro-evergreen");
            var versionInformation = updator.GetCurrentAddInVersion();

            var version = new Notification {
                Message = versionInformation.ToString(),
                ImageUrl = "",
                Title = "Add in Version Information"
            };

            FrameworkApplication.AddNotification(version);
        }
    }
}
