using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class ElementFactory {

	public Element DefaultElement { get; set; }

	public ElementFactory() {
		DefaultElement = new StorySumthing("New Sumthing", Category.NONE);
	}

	public ElementFactory(Category category) {
		DefaultElement = new StorySumthing(category.Name, category);
	}

	public Element CreateSumthing(Category category) {
		string name = "New " + category.Name;
		return CreateSumthing(name, category);
	}

	public Element CreateSumthing(string name, Category category) {
		return new StorySumthing(name, category);
	}
}
