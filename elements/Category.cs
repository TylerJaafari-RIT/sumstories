using System.Runtime.CompilerServices;

namespace sumstories.elements;

public class Category {
	public static readonly Category NONE = new Category(0, "None", []);

	public static Dictionary<string, Category> Defaults = new Dictionary<string, Category>() {
		{ "none", NONE },
		{ "character", new Category(1, "Character", [
			new TextAttribute("Full Name"),
			new NumberAttribute("Age", "years")
			])
		},
	};

	static readonly string[] DefaultsList = { "none", "character" };

	public static Category GetDefaultCategoryById(long id) {
		if (Defaults.TryGetValue(DefaultsList[id], out Category? value)) {
			return value;
		} else {
			Console.WriteLine($"No default category of ID {id} found.");
			return NONE;
		}
	}

	public readonly int ID;
	public string Name { get; set; }
	public IAttribute [] DefaultAttributes { get; }

	public Category(int ID, string Name) {
		this.ID = ID;
		this.Name = Name;
		this.DefaultAttributes = new IAttribute[1];
	}

	public Category(int ID, string Name, IAttribute [] DefaultAttributes) {
		this.ID = ID;
		this.Name = Name;
		this.DefaultAttributes = DefaultAttributes;
	}

	public override string ToString() {
		return this.Name;
	}
}