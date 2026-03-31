namespace sumstories.elements;

using System.Collections.Generic;

public class StorySumthing: Element {
	public List<IAttribute> Attributes { get; }

	public void AddAttribute(IAttribute attribute) {
		Attributes.Add(attribute);
	}

	public bool HasAttribute(string attributeName) {
		IAttribute? att = GetAttribute(attributeName);
		if (att == null) return false;
		else return true;
	}

	public IAttribute? GetAttribute(string attributeName) {
		foreach (IAttribute att in Attributes) {
			if (att.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase)) {
				return att;
			}
		}
		return null;
	}

	public IAttribute? RemoveAttribute(string attributeName) {
		IAttribute? att = GetAttribute(attributeName);
		if (att != null) Attributes.Remove(att);
		return att;
	}

	public StorySumthing(int ID, Category Category): this(ID, "New " + Category.Name, Category) {	}

	public StorySumthing(int ID, string Name, Category Category): base(ID) {
		this.Name = Name;
		this.Category = Category;
		//Attributes = [.. Category.DefaultAttributes]; // simplified list init
		// this language has so many neat shortcuts
		// Edit: too bad we can't even use it here T_T

		Attributes = [];
		foreach (IAttribute attribute in Category.DefaultAttributes) {
			Attributes.Add(attribute.Clone());
		}
	}

	public override string ToString() {
		string attributeDescriptions = "";
		foreach (IAttribute attribute in Attributes) {
			attributeDescriptions += attribute + "\n";
		}
		return $"ID: {ID}\nName: {Name}\nCategory: {Category}\nAttributes:\n{attributeDescriptions}";
	}
}