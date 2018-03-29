namespace ProEvergreen.ProAddIn {
    using ArcGIS.Desktop.Framework;
    using ArcGIS.Desktop.Framework.Contracts;

    internal class Module1 : Module {
        private static Module1 _this;

        /// <summary>
        ///     Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ?? (_this = (Module1) FrameworkApplication.FindModule("ProAddIn_Module"));

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

        public string Version() {
            var updator = new SelfUpdate("agrc", "TrailsAddin");
            var version = updator.GetCurrentAddInVersion();

            var a = new Notification {
                Message = version.ToString(),
                ImageUrl = "",
                Title = "Add in Version Information"
            };

            FrameworkApplication.AddNotification(a);

            return version.ToString();
        }

        #endregion Overrides
    }
}
