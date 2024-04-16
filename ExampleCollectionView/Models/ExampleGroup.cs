using System;
namespace ExampleCollectionView.Models
{
	public class ExampleGroup(string key, List<Example> items) : List<Example>(items)
	{
		public string Key { get; private set; } = key;

		public override string ToString()
		{
			return Key;
		}
	}
}

