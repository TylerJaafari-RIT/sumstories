namespace sumstories.elements;

using System.Collections.Generic;

public class Folder: Element {
	List<Element> Items { get; }

	public void AddItem(Element item) { Items.Add(item); }

	public void RemoveItem(Element item) { Items.Remove(item); }

	public void RemoveItem(string name) {
		// TODO: implement
	}
}