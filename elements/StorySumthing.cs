namespace sumstories.elements;

using System.Collections.Generic;

public class StorySumthing : Element {
    public List<IAttribute> Attributes { get; }

    public void AddAttribute(IAttribute attribute) {
        Attributes.Add(attribute);
	}

    public StorySumthing(Category Category) {
        this.Name = "New " + Category.Name;
        this.Category = Category;
        Attributes = [.. Category.DefaultAttributes];
    }

    public StorySumthing(string Name, Category Category) {
        this.Name = Name;
        this.Category = Category;
        Attributes = [.. Category.DefaultAttributes]; // simplified list init
        // this language has so many neat shortcuts
    }
}