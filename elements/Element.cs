namespace sumstories.elements;

public abstract class Element(long ID) {
	public readonly long ID = ID;
	public string Name { get; set; } = "New Element";

	public Category Category { get; set; } = Category.NONE;
}