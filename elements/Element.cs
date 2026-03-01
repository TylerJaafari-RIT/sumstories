namespace elements;
using System.Collections.Generic;

public class Element : IElement {
    public string Name { get; set; }
    public Category Category { get; set; }

    public List<Attribute> Attributes { get; }

    public void addAttribute(Attribute attribute) {
        Attributes.Add(attribute);
	}
}