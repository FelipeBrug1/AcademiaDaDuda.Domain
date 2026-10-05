// Felipe Antonio Brüggemann

using AcademiaDaDuda.Presentation.AppMaui;
using Android.App;
using Android.Runtime;

namespace AcademiaDaDuda.Presentation.AppMaui
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
