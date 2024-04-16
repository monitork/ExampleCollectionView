using ExampleCollectionView.Views;

namespace ExampleCollectionView;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("Example", typeof(ExamplePage));

	}
}
