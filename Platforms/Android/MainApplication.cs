using Android.App;
using Android.Runtime;
using CollectionViewIssues.Components;
using CollectionViewIssues.Platforms.Android.Handler;

namespace CollectionViewIssues;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		return MauiProgram.CreateMauiApp();
	}

	
}
