namespace sumstories.elements;

using System.Collections.Generic;

public class Folder: Element {
	public List<Element> Items { get; }

	public Folder(long ID, string Name, Category Category): base(ID) {
		this.Name = Name;
		this.Category = Category;
		this.Items = new List<Element>();
	}

	public Folder(long ID): this(ID, "New Folder", Category.NONE) { }

	public Folder(long ID, string Name): this(ID, Name, Category.NONE) {	}

	public Folder(long ID, Category Category): this(ID, "New Folder", Category) { }

	public Element? GetItemById(long ID) {
		foreach (Element item in Items) {
			if (item.ID == ID) {
				return item;
			}
		}
		return null;
	}

	public void AddItem(Element item) {
		if (this.Category.Equals(Category.NONE) || this.Category.Equals(item.Category))
			Items.Add(item);
		else
			Console.Error.WriteLine($"This folder is for {Category} elements and cannot store {item.Category} elements.");
	}

	public void RemoveItem(Element item) {
		if (Items.Remove(item)) {
			Console.WriteLine("Item removed.");
		} else {
			Console.Error.WriteLine("Item is not in this folder");
		}
	}

    public void RemoveItem(long ID) {
		bool itemFound = false;
		foreach (Element item in Items) {
			if (item.ID == ID) {
				itemFound = true;
				RemoveItem(item);
				break;
			}
		}
		if (!itemFound) {
			Console.Error.WriteLine("Item is not in this folder.");
		}
	}

	public void RemoveItem(string name) {
		// TODO: implement
	}

	public override string ToString() {
		string itemList = "";
		foreach (Element item in Items) {
			itemList += item.ID + ": " + item.Name + "\n";
		}
		return $"ID: {ID}\nName: {Name}\nCategory: {Category}\nItems: {itemList}";
	}
}