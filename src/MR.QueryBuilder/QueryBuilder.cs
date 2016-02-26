using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MR.QueryBuilder.Metadata;

namespace MR.QueryBuilder
{
	public class QueryBuilder<T>
	{
		private enum Kind
		{
			Select,
			Count,
			Delete
		}

		private class JoinClause
		{
			public JoinClause(Type type, string kind, string joiner, Action<QueryTableConfiguration> configure)
			{
				Type = type;
				Kind = kind;
				Joiner = joiner.Trim();
				Configure = configure ?? _noop;
			}

			public Type Type { get; }
			public string Kind { get; }
			public Action<QueryTableConfiguration> Configure { get; }
			public string Joiner { get; }
		}

		private static Action<QueryTableConfiguration> _noop = (c) => { };

		private List<JoinClause> _joinClauses = new List<JoinClause>();
		private List<string> _whereClauses = new List<string>();
		private string _skip;
		private string _take;
		private string _order;
		private bool _desc;

		public QueryBuilder()
		{
		}

		public string Select(string table, string selector = null, Action<QueryTableConfiguration> configure = null)
		{
			var sb = new StringBuilder();
			configure = configure ?? _noop;

			var names = default(string);
			if (selector == null)
			{
				names = GenerateNames(table, configure);
			}
			sb.Append($"SELECT {selector?.Trim()} {names?.Trim()}").AppendLine().Append("FROM ").Append(table);
			AppendCore(sb, Kind.Select);

			return sb.ToString();
		}

		public string Count(string table)
		{
			var sb = new StringBuilder();

			sb.Append("SELECT COUNT(*)").AppendLine().Append("FROM ").Append(table);
			AppendCore(sb, Kind.Count);

			return sb.ToString();
		}

		public string Delete(string table)
		{
			var sb = new StringBuilder();

			sb.Append($"DELETE FROM {table}");
			AppendCore(sb, Kind.Delete);

			return sb.ToString();
		}

		public QueryBuilder<T> InnerJoin<TJoined>(string joiner, Action<QueryTableConfiguration> configure = null)
		{
			if (string.IsNullOrWhiteSpace(joiner))
				throw new ArgumentException(nameof(joiner));

			_joinClauses.Add(new JoinClause(
				typeof(TJoined),
				"INNER JOIN",
				joiner,
				configure));

			return this;
		}

		public QueryBuilder<T> LeftJoin<TJoined>(string joiner, Action<QueryTableConfiguration> configure = null)
		{
			if (string.IsNullOrWhiteSpace(joiner))
				throw new ArgumentException(nameof(joiner));

			_joinClauses.Add(new JoinClause(
				typeof(TJoined),
				"LEFT OUTER JOIN",
				joiner,
				configure));

			return this;
		}

		public QueryBuilder<T> RightJoin<TJoined>(string joiner, Action<QueryTableConfiguration> configure = null)
		{
			if (string.IsNullOrWhiteSpace(joiner))
				throw new ArgumentException(nameof(joiner));

			_joinClauses.Add(new JoinClause(
				typeof(TJoined),
				"RIGHT OUTER JOIN",
				joiner,
				configure));

			return this;
		}

		public QueryBuilder<T> Where(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
				throw new ArgumentException(nameof(val));

			_whereClauses.Add(val);

			return this;
		}

		public QueryBuilder<T> OrderBy(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
				throw new ArgumentException(nameof(val));

			_order = val;

			return this;
		}

		public QueryBuilder<T> Desc()
		{
			_desc = true;
			return this;
		}

		public QueryBuilder<T> Skip(string val)
		{
			_skip = val;
			return this;
		}

		public QueryBuilder<T> Take(string val)
		{
			_take = val;
			return this;
		}

		private void AppendCore(StringBuilder sb, Kind kind)
		{
			if (kind == Kind.Select || kind == Kind.Count)
			{
				for (int i = 0; i < _joinClauses.Count; i++)
				{
					var join = _joinClauses[i];
					sb.AppendLine().Append(join.Kind).Append(" ").Append(join.Joiner);
				}
			}

			for (int i = 0; i < _whereClauses.Count; i++)
			{
				if (i == 0) sb.AppendLine().Append("WHERE ");
				else sb.Append(" AND ");
				sb.Append(_whereClauses[i].Trim());
			}

			if (kind == Kind.Select)
			{
				if (_order != null)
				{
					sb.AppendLine().Append("ORDER BY ").Append(_order.Trim());
					if (_desc) sb.Append(" DESC");
				}

				if (_skip == null && _take != null)
				{
					_skip = "0";
				}
				if (_skip != null)
				{
					sb.AppendLine().AppendFormat("OFFSET {0} ROWS", _skip);
				}
				if (_take != null)
				{
					sb.AppendFormat(" FETCH NEXT {0} ROWS ONLY", _take);
				}
			}
		}

		private string GenerateNames(string table, Action<QueryTableConfiguration> configure)
		{
			var sb = new StringBuilder();
			sb.Append(GenerateNamesFor(typeof(T), ExtractPrefix(table), configure));
			for (int i = 0; i < _joinClauses.Count; i++)
			{
				var join = _joinClauses[i];
				var names = GenerateNamesFor(join.Type, ExtractPrefix(join.Joiner), join.Configure);
				sb.Append(", ").Append(names);
			}
			return sb.ToString();
		}

		private string ExtractPrefix(string joiner)
		{
			var splitted = joiner.Split(' ');
			if (splitted.Length == 1)
			{
				return null;
			}
			return splitted[1];
		}

		private string GenerateNamesFor(Type type, string prefix, Action<QueryTableConfiguration> configure)
		{
			var config = new QueryTableConfiguration();
			configure(config);
			return GenerateProperties(
				type,
				prefix,
				config.Excluded,
				config.ExcludedAll ? string.Empty : config.Included,
				config.PrimaryKeyName);
		}

		public string GenerateProperties(Type model, string prefix = null, string propsToExclude = null, string propsToInclude = null, string keyName = "Id")
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			if (string.IsNullOrWhiteSpace(keyName))
				throw new ArgumentException(nameof(keyName));

			var properties = MetadataService.Default.GetProperties(model);
			var sb = new StringBuilder();

			prefix = prefix?.Trim();
			var included = propsToInclude == null ? null : propsToInclude.Split(',').Select(s => s.Trim().ToUpper()).ToArray();
			var excluded = (propsToExclude ?? string.Empty).Split(',').Select(s => s.Trim().ToUpper()).ToArray();
			keyName = keyName?.Trim();

			AppendPrefix(prefix, sb);
			sb.Append(keyName);

			foreach (var property in properties)
			{
				var name = property.Name;
				if (IsComplexType(property.PropertyType) ||
					name == keyName ||
					excluded.Contains(name.ToUpper()))
				{
					continue;
				}

				if (included != null &&
					included.Contains(name.ToUpper()))
				{
					AppendName(sb, prefix, name);
				}
				else if (included != null)
				{
					continue;
				}

				AppendName(sb, prefix, name);
			}

			return sb.ToString();
		}

		private StringBuilder AppendPrefix(string prefix, StringBuilder sb)
		{
			if (!string.IsNullOrWhiteSpace(prefix))
			{
				sb.Append(prefix).Append('.');
			}
			return sb;
		}

		private StringBuilder AppendName(StringBuilder sb, string prefix, string name)
		{
			sb.Append(", ");
			AppendPrefix(prefix, sb);
			sb.Append(name);
			return sb;
		}

		private bool IsComplexType(Type type)
		{
			var typeInfo = type.GetTypeInfo();
			return (typeInfo.IsClass && type != typeof(string));
		}
	}
}
