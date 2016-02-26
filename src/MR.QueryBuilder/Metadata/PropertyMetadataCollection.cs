using System.Collections;
using System.Collections.Generic;

namespace MR.QueryBuilder.Metadata
{
	public class PropertyMetadataCollection : IReadOnlyCollection<PropertyMetadata>
	{
		private List<PropertyMetadata> _list;

		public PropertyMetadataCollection(List<PropertyMetadata> list)
		{
			_list = list;
		}

		public int Count => _list.Count;

		public IEnumerator<PropertyMetadata> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
	}
}
