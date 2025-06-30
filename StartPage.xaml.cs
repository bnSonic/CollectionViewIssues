using CollectionViewIssues.Pages;

namespace CollectionViewIssues;

public partial class StartPage : ContentPage
{
	public StartPage()
	{
		InitializeComponent();
	}

	private async void Demo1_Tapped(object sender, TappedEventArgs e)
	{
		var demo1Page = new Demo1Page();
		await Navigation.PushAsync(demo1Page);
    }
}