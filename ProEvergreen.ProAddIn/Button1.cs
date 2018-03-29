namespace ProEvergreen.ProAddIn {
    using ArcGIS.Desktop.Framework.Contracts;

    internal class Button1 : Button {
        protected override void OnClick() {
            Module1.Current.Version();
        }
    }
}
