namespace ProEvergreen.AddIn
{
    using ArcGIS.Desktop.Framework.Contracts;

    internal class UpdateButton : Button
    {
        protected override async void OnClick()
        {
            await AddinModule.Current.Update();
        }
    }
}
