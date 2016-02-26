namespace MR.QueryBuilder
{
	public class QueryTableConfiguration
	{
		public string PrimaryKeyName { get; private set; } = "Id";
		public string Included { get; private set; }
		public string Excluded { get; private set; }
		public bool ExcludedAll { get; private set; }

		public static QueryTableConfiguration Default { get; } = new QueryTableConfiguration();

		public QueryTableConfiguration KeyName(string name)
		{
			PrimaryKeyName = name;
			return this;
		}

		public QueryTableConfiguration Include(string columnsToInclude)
		{
			Included = columnsToInclude;
			return this;
		}

		public QueryTableConfiguration Exclude(string columnsToExclude)
		{
			Excluded = columnsToExclude;
			return this;
		}

		public QueryTableConfiguration ExcludeAll()
		{
			ExcludedAll = true;
			return this;
		}
	}
}
