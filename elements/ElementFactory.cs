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
		DefaultElement = new StorySumthing(category);
	}

	public ElementFactory(Element element) {
		DefaultElement = element;
	}

	public Element CreateSumthing(Category category) {
		string name = "New " + category.Name;
		return CreateSumthing(name, category);
	}

	public Element CreateSumthing(string name, Category category) {
		if (DefaultElement is Folder)
			return new Folder(name, category);
		else
			return new StorySumthing(name, category);
	}
}
