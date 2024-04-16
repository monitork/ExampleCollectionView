namespace ExampleCollectionView.Views;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private async void OnExampeClicked(object sender, EventArgs e)
	{
		Console.WriteLine("[SHIN] OnExampeClicked");
		await Shell.Current.GoToAsync($"/Example");
	}
}

