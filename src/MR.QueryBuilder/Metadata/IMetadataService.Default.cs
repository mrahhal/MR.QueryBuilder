using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace MR.QueryBuilder.Metadata
{
	public class DefaultMetadataService : IMetadataService
	{
		private ConcurrentDictionary<Type, PropertyMetadataCollection> _cache =
			new ConcurrentDictionary<Type, PropertyMetadataCollection>();

		private BindingFlags _flags =
			BindingFlags.Public | BindingFlags.Instance;

		public PropertyMetadataCollection GetProperties(Type model)
			=> _cache.GetOrAdd(model, GetPropertiesCore);

		private PropertyMetadataCollection GetPropertiesCore(Type model)
		{
			var properties = model.GetProperties(_flags);
			var list = properties
				.Select(pi => new PropertyMetadata(pi))
				.ToList();
			return new PropertyMetadataCollection(list);
		}
	}
}
