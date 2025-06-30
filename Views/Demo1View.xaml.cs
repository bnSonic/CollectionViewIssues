using CollectionViewIssues.ViewModels;
using CollectionViewIssues.Views.Cells;

namespace CollectionViewIssues.Views;

public partial class Demo1View : ContentView
{
	MyDataList _myDataList;

	public Demo1View()
	{
		InitializeComponent();

	}
	protected override void OnBindingContextChanged()
	{
		// for debuggin purposes - see when and if we got the correct data
		base.OnBindingContextChanged();
		if (BindingContext != null && BindingContext is MyDataList data)
		{
			_myDataList = data;
			_myDataList.MyCollectionView = xamlCollectionView;
		}
	}

	
}

internal class MyDataTemplateSelector : DataTemplateSelector
{
	private readonly DataTemplate _dataTemplate;
	private readonly DataTemplate _groupTemplate;

	public MyDataTemplateSelector()
	{

		_dataTemplate = new DataTemplate(typeof(DataEntryView));
		_groupTemplate = new DataTemplate(typeof(GroupEntrySwipeView));

		//-- you can try the simple Entry-View which works better (or correct? i wasn't able to reproduce the problem with this simple one)
		//_dataTemplate = new DataTemplate(typeof(DataEntryViewSimple));

		//-- you can try the simple group-View without swipe etc. it doesn't help
		//_groupTemplate = new DataTemplate(typeof(GroupEntryViewSimple));
	}
	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		if (item is IMyData data)
		{
			if (data.DataType == MyDataType.Data)
			{
				return _dataTemplate;
			}
			else if (data.DataType == MyDataType.Group)
			{
				return _groupTemplate;
			}
		}
		//fallback
		return _dataTemplate; 
	}
}