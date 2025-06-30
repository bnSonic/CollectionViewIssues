using System.Collections;

namespace CollectionViewIssues.Components;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MyCollectionView : ContentView
{
	#region Bindable Properties
	public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource),
		typeof(IEnumerable), typeof(MyCollectionView), defaultBindingMode: BindingMode.OneWay);
	public IEnumerable ItemsSource
	{
		get => (IEnumerable)GetValue(ItemsSourceProperty);
		set
		{
			SetValue(ItemsSourceProperty, value);
		}
	}


	public static BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate),
		typeof(DataTemplate), typeof(MyCollectionView),
		defaultBindingMode: BindingMode.OneWay);
	public DataTemplate ItemTemplate
	{
		get => (DataTemplate)GetValue(ItemTemplateProperty);
		set
		{
			SetValue(ItemTemplateProperty, value);
		}
	}


	public static BindableProperty NoDataTextProperty = BindableProperty.Create(nameof(NoDataText),
		typeof(string), typeof(MyCollectionView), defaultBindingMode: BindingMode.OneWay);
	public string NoDataText
	{
		get { return (string)GetValue(NoDataTextProperty); }
		set { SetValue(NoDataTextProperty, value); }
	}

	public static BindableProperty NoDataColorProperty = BindableProperty.Create(nameof(NoDataColor),
		typeof(Color), typeof(MyCollectionView), defaultBindingMode: BindingMode.OneWay);
	public Color NoDataColor
	{
		get { return (Color)GetValue(NoDataColorProperty); }
		set { SetValue(NoDataColorProperty, value); }
	}

	public static BindableProperty ShowNoDataTextProperty = BindableProperty.Create(nameof(ShowNoDataText),
		typeof(bool), typeof(MyCollectionView), defaultValue: false, defaultBindingMode: BindingMode.OneWay);
	public bool ShowNoDataText
	{
		get { return (bool)GetValue(ShowNoDataTextProperty); }
		set { SetValue(ShowNoDataTextProperty, value); }
	}
	#endregion

	public MyCollectionView()
	{
		InitializeComponent();
	}

	public CollectionView CollectionView => this.xamlCollectionView;

	public object CurrentRevealedSwipeGrid { get; set; }


	private int ItemsCount(IEnumerable source)
	{
		int result = 0;
		if (source == null)
			return result;

		var enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
			result++;

		return result;
	}
}