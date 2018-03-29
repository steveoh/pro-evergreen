namespace ProEvergreen.AddIn {
    using ArcGIS.Desktop.Framework.Contracts;

    internal class GetVersionButton : Button {
        protected override void OnClick() {
            AddinModule.Current.ShowVersion();
        }
    }
}
