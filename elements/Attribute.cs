namespace sumstories.elements;

public interface IAttribute {
	public string Name { get; set; }

	public IAttribute Clone();
}