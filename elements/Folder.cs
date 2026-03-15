namespace sumstories.elements;

using System.Collections.Generic;

public class Folder: Element {
	public List<Element> Items { get; }

	public void AddItem(Element item) { 
		if (this.Category.Equals(Category.NONE) || this.Category.Equals(item.Category))
			Items.Add(item);
		else
			Console.Error.WriteLine($"This folder is for {Category} elements and cannot store a {item.Category}");
	}

	public void RemoveItem(Element item) { Items.Remove(item); }

	public void RemoveItem(string name) {
		// TODO: implement
	}
}