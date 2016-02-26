using System.Text.RegularExpressions;
using Xunit;

namespace MR.QueryBuilder
{
	public class QueryBuilderTest
	{
		[Fact]
		public void Works()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.Where("UserName = 'root'")
				.Skip(1).Take(10)
				.Select("AspNetUsers u", null, c => c.Exclude("Name"));

			Assert.NotNull(query);
		}

		[Fact]
		public void Select()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.WhereNotNull("f.Name")
				.Select("Foo f");

			var expected = @"
				SELECT f.Id, f.Name
				FROM Foo f
				WHERE f.Name IS NOT NULL";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_SkipTake()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.Skip(2).Take(3)
				.Select("Foo f");

			var expected = @"
				SELECT f.Id, f.Name
				FROM Foo f
				OFFSET 2 ROWS FETCH NEXT 3 ROWS ONLY";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_WithoutPrefix()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.WhereNotNull("Name")
				.Select("Foo");

			var expected = @"
				SELECT Id, Name
				FROM Foo
				WHERE Name IS NOT NULL";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_Order()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.OrderBy("Name")
				.Select("Foo");

			var expected = @"
				SELECT Id, Name
				FROM Foo
				ORDER BY Name";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_OrderDesc()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.OrderBy("Name").Desc()
				.Select("Foo");

			var expected = @"
				SELECT Id, Name
				FROM Foo
				ORDER BY Name DESC";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_IncludeProps()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.Select("Foo", null, c =>
				{
					c.Include("Id");
				});

			var expected = @"
				SELECT Id
				FROM Foo";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_ExcludeProps()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.Select("Foo", null, c =>
				{
					c.Exclude("Name");
				});

			var expected = @"
				SELECT Id
				FROM Foo";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_ExcludeAll()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.Select("Foo", null, c =>
				{
					c.ExcludeAll();
					c.Include("Id, Name");
				});

			var expected = @"
				SELECT Id
				FROM Foo";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_Join()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.InnerJoin<Model2>("Bar b on b.Id = f.BarId")
				.Select("Foo f");

			var expected = @"
				SELECT f.Id, f.Name, b.Id, b.Peer
				FROM Foo f
				INNER JOIN Bar b on b.Id = f.BarId";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_Join_Exclude()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.InnerJoin<Model2>("Bar b on b.Id = f.BarId", c =>
				{
					c.Exclude("Peer");
				})
				.Select("Foo f");

			var expected = @"
				SELECT f.Id, f.Name, b.Id
				FROM Foo f
				INNER JOIN Bar b on b.Id = f.BarId";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_CaseInsensetive()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.InnerJoin<Model2>("Bar b on b.Id = f.BarId", c =>
				{
					c.Exclude("peer");
				})
				.Select("Foo f");

			var expected = @"
				SELECT f.Id, f.Name, b.Id
				FROM Foo f
				INNER JOIN Bar b on b.Id = f.BarId";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_WithSelector()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.WhereNotNull("Name")
				.Select("Foo", "Id, Name");

			var expected = @"
				SELECT Id, Name
				FROM Foo
				WHERE Name IS NOT NULL";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Select_Count()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.WhereNotNull("Name")
				.Count("Foo");

			var expected = @"
				SELECT COUNT(*)
				FROM Foo
				WHERE Name IS NOT NULL";

			AssertEqual(expected, query);
		}

		[Fact]
		public void Delete()
		{
			var qb = new QueryBuilder<Model1>();
			var query = qb
				.WhereNull("Name")
				.Delete("Foo");

			var expected = @"
				DELETE FROM Foo
				WHERE Name IS NULL";

			AssertEqual(expected, query);
		}

		private class Model1
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		private class Model2
		{
			public int Id { get; set; }
			public string Peer { get; set; }
		}

		private string Collapse(string value)
		{
			value = value.Trim();
			var regex = new Regex("[\r\n\t ]+");
			value = regex.Replace(value, " ");
			return value;
		}

		private void AssertEqual(string expected, string actual)
		{
			Assert.Equal(Collapse(expected), Collapse(actual));
		}
	}
}
