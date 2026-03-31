namespace sumstories.elements;

public abstract class Element(int ID) {
	public readonly int ID = ID;
	public string Name { get; set; } = "New Element";

	public Category Category { get; set; } = Category.NONE;
}