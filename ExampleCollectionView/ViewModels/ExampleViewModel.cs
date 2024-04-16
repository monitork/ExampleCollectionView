using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExampleCollectionView.Base;
using ExampleCollectionView.Models;

namespace ExampleCollectionView.ViewModels
{
	public class ExampleViewModel : ObservableObject
	{

		private CancellationTokenSource _throttleCts = new CancellationTokenSource();


		public ObservableRangeCollection<ExampleGroup> Items { get; private set; } = [];

		private List<ExampleGroup>? _cachedItems;

		public string? SearchTerm { get; set; }


		public void Initialization()
		{
			CreateCollection();
		}

		public void Search(string? text)
		{
			_ = DebouncedSearch();
		}

		private async void CreateCollection()
		{
			// create cache data
			var exams = await Task.Run(() =>
			   {
				   List<string> groups = ["A", "B", "C"];
				   List<ExampleGroup> exams = [];
				   foreach (string group in groups)
				   {
					   List<Example> examples = [];
					   var groupName = group;
					   if (groupName == "A")
					   {
						   groupName = "Alexander";
					   }
					   else if (groupName == "B")
					   {
						   groupName = "BExample";
					   }
					   for (int index = 0; index <= 50; index++)
					   {
						   examples.Add(new Example
						   {
							   Name = $"{groupName}{index}"
						   });
					   }
					   exams.Add(new ExampleGroup(groupName, examples));
				   }
				   return exams;
			   });
			_cachedItems = exams;
			Items.AddRange(exams);

		}

		private async Task FilterSearch()
		{
			// Filter cached data.
			var exams = await Task.Run(() =>
			{
				var Term = SearchTerm ?? "";
				var items = (_cachedItems ?? []).Select((res) =>
				{
					var filters = res.Where((e) => e.Name.Contains(Term)).ToList();
					return new ExampleGroup(res.Key, filters);
				}).Where((res) => res.Count > 0);

				return items;
			});
			Items.Clear();
			Items.AddRange(exams);
		}


		private async Task DebouncedSearch()
		{
			try
			{
				await Task.Delay(TimeSpan.FromMilliseconds(150), _throttleCts.Token)
				.ContinueWith(async task => await FilterSearch(),
					CancellationToken.None,
					TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.FromCurrentSynchronizationContext());
			}
			catch
			{
				Console.WriteLine("error");
			}
		}

	}
}

