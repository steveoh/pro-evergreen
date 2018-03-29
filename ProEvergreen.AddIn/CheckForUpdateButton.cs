namespace ProEvergreen.AddIn {
    using ArcGIS.Desktop.Framework.Contracts;

    internal class CheckForUpdateButton : Button {
        protected override async void OnClick() {
            await AddinModule.Current.CheckForUpdate();
        }
    }
}
