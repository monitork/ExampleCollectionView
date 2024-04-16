using System;
namespace ExampleCollectionView.Models
{
	public class Example
	{
		public required string Name { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}
}

