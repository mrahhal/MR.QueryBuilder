using System.Linq;
using FluentAssertions;
using Xunit;

namespace MR.QueryBuilder.Metadata
{
	public class MetadataServiceTest
	{
		private class Model1
		{
			public int Foo { get; set; }
			public int Bar { get; set; }
		}

		private class Model2
		{
			public int Foo { get; set; }
			public int Bar { get; set; }
			private int Baz { get; set; }
		}

		private class Model3 : Model2
		{
			public int Some { get; set; }
		}

		private DefaultMetadataService Service => MetadataService.Default;

		[Fact]
		public void GetProperties()
		{
			var properties = Service.GetProperties<Model1>();
			AssertProperties(properties, "Foo", "Bar");
		}

		[Fact]
		public void GetProperties_IgnoresNonPublicProperties()
		{
			var properties = Service.GetProperties<Model2>();
			AssertProperties(properties, "Foo", "Bar");
		}

		[Fact]
		public void GetProperties_FindsAncestorProperties()
		{
			var properties = Service.GetProperties<Model3>();
			AssertProperties(properties, "Foo", "Bar", "Some");
		}

		private void AssertProperties(PropertyMetadataCollection collection, params string[] properties)
		{
			var names = collection.Select(p => p.Name).ToArray();
			names.Should().BeEquivalentTo(properties);
		}

		private string Aggregate(PropertyMetadataCollection properties)
			=> properties.Select(p => p.Name).Aggregate((_1, _2) => _1 + "," + _2);
	}
}
