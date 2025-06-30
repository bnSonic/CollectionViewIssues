using CollectionViewIssues.ViewModels;

namespace CollectionViewIssues.Pages;

public partial class Demo1Page : ContentPage
{
	MyDataList _myDataList;
	public Demo1Page()
	{
		InitializeComponent();
		_myDataList = new MyDataList();

		BindingContext = _myDataList;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		//-- load data 
		_myDataList.LoadData();
    }
}