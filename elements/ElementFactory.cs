using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class ElementFactory {
	Element DefaultElement { get; set; }

	List<Attribute> Attributes { get; set; }

	public Element CreateSumthing(Category category) {
		string name = "New " + category.Name;
		return CreateSumthing(name, category);
	}

	public Element CreateSumthing(string name, Category category) {
		return new StorySumthing(name, category);
	}
}
