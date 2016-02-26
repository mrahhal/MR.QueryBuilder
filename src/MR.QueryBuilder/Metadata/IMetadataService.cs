using System;

namespace MR.QueryBuilder.Metadata
{
	public interface IMetadataService
	{
		PropertyMetadataCollection GetProperties(Type type);
	}

	public static class MetadataServiceExtensions
	{
		public static PropertyMetadataCollection GetProperties<T>(this IMetadataService @this)
			=> @this.GetProperties(typeof(T));
	}
}
