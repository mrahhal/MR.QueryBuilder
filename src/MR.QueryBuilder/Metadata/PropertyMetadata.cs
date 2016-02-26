using System;
using System.Reflection;

namespace MR.QueryBuilder.Metadata
{
	public class PropertyMetadata
	{
		public PropertyMetadata(PropertyInfo pi)
		{
			PropertyType = pi.PropertyType;
			Name = pi.Name;
		}

		public Type PropertyType { get; }

		public string Name { get; }
	}
}
