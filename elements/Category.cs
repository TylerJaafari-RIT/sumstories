namespace sumstories.elements;

public class Category {
	public static readonly Category NONE = new Category("None", []);
	public string Name { get; set; }
	public Attribute [] DefaultAttributes { get; }

	public Category(string Name) {
		this.Name = Name;
		this.DefaultAttributes = new Attribute[1];
	}

	public Category(string Name, Attribute [] DefaultAttributes) {
		this.Name = Name;
		this.DefaultAttributes = DefaultAttributes;
	}
}