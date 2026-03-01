namespace elements;

using System.Collections.Generic;

public class Folder: IElement {
	List<Element> Items { get; }

	public void addItem(IElement item) { Items.Add(item); }

	public void removeItem(IElement item) { Items.Remove(item); }

	public void removeItem(string name) {
		// TODO: implement
	}
}