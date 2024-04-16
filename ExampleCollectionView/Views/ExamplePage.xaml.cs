using ExampleCollectionView.ViewModels;

namespace ExampleCollectionView.Views;

public partial class ExamplePage : ContentPage
{
	ExampleViewModel ViewModel;
	bool IsInitial = false;
	public ExamplePage()
	{
		InitializeComponent();
		ViewModel = new ExampleViewModel();
		BindingContext = ViewModel;
		if(DeviceInfo.Current.Idiom == DeviceIdiom.Tablet){
			collectionView.ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical);
		} else {
			collectionView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
		}
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();
		if (IsInitial)
			return;

		ViewModel.Initialization();
	}

	void OnTextChanged(System.Object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
	{
		ViewModel.Search(e.NewTextValue);
	}
}
