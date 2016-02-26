namespace MR.QueryBuilder
{
	public static class QueryBuilderExtensions
	{
		public static QueryBuilder<T> WhereFalse<T>(this QueryBuilder<T> @this, string val)
		{
			return @this.Where(val + " = 0");
		}

		public static QueryBuilder<T> WhereTrue<T>(this QueryBuilder<T> @this, string val)
		{
			return @this.Where(val + " = 1");
		}

		public static QueryBuilder<T> WhereNull<T>(this QueryBuilder<T> @this, string val)
		{
			return @this.Where(val + " IS NULL");
		}

		public static QueryBuilder<T> WhereNotNull<T>(this QueryBuilder<T> @this, string val)
		{
			return @this.Where(val + " IS NOT NULL");
		}

		public static QueryBuilder<T> Skip<T>(this QueryBuilder<T> @this, int val)
		{
			return @this.Skip(val.ToString());
		}

		public static QueryBuilder<T> Take<T>(this QueryBuilder<T> @this, int val)
		{
			return @this.Take(val.ToString());
		}
	}
}