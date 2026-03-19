namespace sumstories.elements;

using System.Collections.Generic;

public class Folder: Element {
	public List<Element> Items { get; }

	public Folder(string Name, Category Category) {
		this.Name = Name;
		this.Category = Category;
		this.Items = new List<Element>();
	}

	public Folder() : this("New Folder", Category.NONE) {	}

	public Folder(string Name) : this(Name, Category.NONE) {	}

	public void AddItem(Element item) {
		if (this.Category.Equals(Category.NONE) || this.Category.Equals(item.Category))
			Items.Add(item);
		else
			Console.Error.WriteLine($"This folder is for {Category} elements and cannot store {item.Category} elements.");
	}

	public void RemoveItem(Element item) { Items.Remove(item); }

	public void RemoveItem(string name) {
		// TODO: implement
	}
}