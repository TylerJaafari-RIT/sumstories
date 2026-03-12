namespace sumstories.elements;
using System.Collections.Generic;

public class StorySumthing : Element {
    public List<Attribute> Attributes { get; }

    public void AddAttribute(Attribute attribute) {
        Attributes.Add(attribute);
	}

    public StorySumthing(string Name, Category Category) {
        this.Name = Name;
        this.Category = Category;
        Attributes = new List<Attribute>();
    }
}