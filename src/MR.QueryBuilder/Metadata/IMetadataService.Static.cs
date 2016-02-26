namespace MR.QueryBuilder.Metadata
{
	public static class MetadataService
	{
		private static DefaultMetadataService _instance;

		static MetadataService()
		{
			_instance = new DefaultMetadataService();
		}

		public static DefaultMetadataService Default => _instance;
	}
}
