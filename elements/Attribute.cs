namespace sumstories.elements;

public interface IAttribute {
	public string Name { get; set; }

	/// <summary>
	/// Creates a deep copy of this attribute.
	/// </summary>
	/// <returns></returns>
	public IAttribute Clone();
}