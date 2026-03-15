namespace sumstories.elements;

public abstract class Element {
	public string Name { get; set; } = "New Element";

	public Category Category { get; set; } = Category.NONE;
}