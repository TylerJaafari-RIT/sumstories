namespace sumstories.elements;
using System.Collections.Generic;

public class StorySumthing : Element {
    public List<Attribute> Attributes { get; }

    public void AddAttribute(Attribute attribute) {
        Attributes.Add(attribute);
	}
}