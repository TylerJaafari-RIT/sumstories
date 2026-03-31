namespace sumstories.elements;

public class Category {
	public static readonly Category NONE = new Category("None", []);

	public static Dictionary<string, Category> Defaults = new Dictionary<string, Category>() {
		{ "none", NONE },
		{ "character", new Category("Character", [
			new TextAttribute("Full Name"),
			new NumberAttribute("Age", "years")
			])
		},
	};

	public string Name { get; set; }
	public IAttribute [] DefaultAttributes { get; }

	public Category(string Name) {
		this.Name = Name;
		this.DefaultAttributes = new IAttribute[1];
	}

	public Category(string Name, IAttribute [] DefaultAttributes) {
		this.Name = Name;
		this.DefaultAttributes = DefaultAttributes;
	}

	public override string ToString() {
		return this.Name;
	}
}