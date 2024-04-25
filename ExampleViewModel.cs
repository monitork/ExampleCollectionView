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
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace ExampleCollectionView.ViewModels
{
	public class ExampleViewModel : ObservableObject
	{

		private CancellationTokenSource _throttleCts = new CancellationTokenSource();


		public ObservableRangeCollection<ExampleGroup> Items { get; set; } = new();

		private List<ExampleGroup>? _cachedItems;

		public string? SearchTerm { get; set; }
		ReplaySubject<string> subject = new ReplaySubject<string>();

		public void Initialization()
		{
			CreateCollection();
			bingUI();
		}

		public void Search(string? text)
		{
			// _ = DebouncedSearch();
			subject.OnNext(text);
			// text.ToObservable()
			// text.ToObservable()
			// 				.SelectMany(v => Observable.Timer(TimeSpan.FromMilliseconds(v)).Select(w => v))
			// 				.ThrottleFirst(TimeSpan.FromMilliseconds(210), Scheduler.Default)
			// 				.Subscribe(x => Console.WriteLine("OnNext: {0}", x));
		}

		public void bingUI()
		{
			// IObservable<T> Debounce<T>(IObservable<T> source, TimeSpan delay, IEqualityComparer<T> comparer = null)
			// {
			// 	return source
			// 	.Do(x => TimestampedPrint($"Source: {x}"))
			// 	.DistinctUntilChanged(comparer ?? EqualityComparer<T>.Default)
			// 	.Select(x => Observable.Return(x).Delay(delay))
			// 	.Switch();
			// }
			// void TimestampedPrint(object o) => Console.WriteLine($"{DateTime.Now:HH.mm.ss.fff}: {o}");
			// Debounce(subject, TimeSpan.FromMilliseconds(500)).Subscribe(x => FilterSearch());
			subject.Throttle(TimeSpan.FromSeconds(1))
			.SubscribeOn(ThreadPoolScheduler.Instance)
			.ObserveOn(new SynchronizationContextScheduler(SynchronizationContext.Current)) // What thread the observation code will happen on -> the GUI thread, thanks to SyncContext
		    .Subscribe(async x => await FilterSearch(x));

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
					   //    Items.Add(new ExampleGroup(groupName, examples));
					   exams.Add(new ExampleGroup(groupName, examples));
				   }
				   return exams;
			   });
			_cachedItems = exams;
			Items.AddRange(exams);

		}

		private int binarySearch(int[] arr, int x)
		{
			int l = 0, r = arr.Length - 1;
			while (l <= r)
			{
				int m = l + (r - l) / 2;

				// Check if x is present at mid
				if (arr[m] == x)
					return m;

				// If x greater, ignore left half
				if (arr[m] < x)
					l = m + 1;

				// If x is smaller, ignore right half
				else
					r = m - 1;
			}

			// If we reach here, then element was
			// not present
			return -1;
		}

		private async Task FilterSearch(string textSearch)
		{
			// Filter cached data.
			var exams = await Task.Run(() =>
			{
				// var Term = SearchTerm ?? "";
				var items = (_cachedItems ?? []).Select((res) =>
				{
					var filters = res.Where((e) => e.Name.Contains(textSearch)).ToList();
					return new ExampleGroup(res.Key, filters);
				}).Where((res) => res.Count > 0);

				return items;
			});
			Items.ReplaceRange(exams);
		}

		string tmpString = "";
		private async Task DebouncedSearch()
		{
			// try
			// {
			// 	DebounceDispatcher debounceDispatcher = new DebounceDispatcher();
			// 	debounceDispatcher.Throttle(300, async _ => await FilterSearch());
			// }
			// catch
			// {
			// 	Console.WriteLine("error");
			// }



		}

	}
}

static class ObservablesEx
{
	public static IObservable<T> ThrottleFirst<T>(this IObservable<T> source,
			TimeSpan timespan, IScheduler timeSource)
	{
		return new ThrottleFirstObservable<T>(source, timeSource, timespan);
	}
}

sealed class ThrottleFirstObservable<T> : IObservable<T>
{
	readonly IObservable<T> source;

	readonly IScheduler timeSource;

	readonly TimeSpan timespan;

	internal ThrottleFirstObservable(IObservable<T> source,
			  IScheduler timeSource, TimeSpan timespan)
	{
		this.source = source;
		this.timeSource = timeSource;
		this.timespan = timespan;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		var parent = new ThrottleFirstObserver(observer, timeSource, timespan);
		var d = source.Subscribe(parent);
		parent.OnSubscribe(d);
		return d;
	}

	sealed class ThrottleFirstObserver : IDisposable, IObserver<T>
	{
		readonly IObserver<T> downstream;

		readonly IScheduler timeSource;

		readonly TimeSpan timespan;

		IDisposable upstream;

		bool once;

		double due;

		internal ThrottleFirstObserver(IObserver<T> downstream,
				IScheduler timeSource, TimeSpan timespan)
		{
			this.downstream = downstream;
			this.timeSource = timeSource;
			this.timespan = timespan;
		}

		public void OnSubscribe(IDisposable d)
		{
			if (Interlocked.CompareExchange(ref upstream, d, null) != null)
			{
				d.Dispose();
			}
		}

		public void Dispose()
		{
			var d = Interlocked.Exchange(ref upstream, this);
			if (d != null && d != this)
			{
				d.Dispose();
			}
		}

		public void OnCompleted()
		{
			downstream.OnCompleted();
		}

		public void OnError(Exception error)
		{
			downstream.OnError(error);
		}

		public void OnNext(T value)
		{
			var now = timeSource.Now.ToUnixTimeMilliseconds();
			if (!once)
			{
				once = true;
				due = now + timespan.TotalMilliseconds;
				downstream.OnNext(value);
			}
			else if (now >= due)
			{
				due = now + timespan.TotalMilliseconds;
				downstream.OnNext(value);
			}

		}
	}
}