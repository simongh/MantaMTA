namespace OpenManta.Core
{
	public abstract class BaseEntity<T>
	{
		public T ID { get; set; }
	}

	public abstract class BaseEntity : BaseEntity<int>
	{
	}

	public abstract class NamedEntity<T> : BaseEntity<T>
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public abstract class NamedEntity : NamedEntity<int>
	{
	}
}