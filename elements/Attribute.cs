namespace sumstories.elements;

public interface IAttribute {
	long ID { get; }
	public string Name { get; set; }

	/// <summary>
	/// Creates a deep copy of this attribute, with a new ID.
	/// </summary>
	/// <returns></returns>
	public IAttribute Clone(long ID);

	public enum Type {
		TEXT,
		NUMBER,
		LIST
	}

	public class DbValues {
		public long id { get; set; }
		public long account { get; set; }
		public string name { get; set; }
		public string text_value { get; set; }
		public int? num_value { get; set; }
		public long? maximum_value { get; set; }
		public int? accuracy { get; set; }
		public long[]? subattributes { get; set; }
		public int type { get; set; }
	}
}