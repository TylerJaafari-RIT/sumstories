namespace elements;

using elements.Element;
using System.Collections.Generic;

public class Folder: IElement {
	List<Element> Items { get; }

	public addItem(IElement item) {	Items.Add(item); }

	public removeItem(IElement item) { Items.Remove(item); }

	public removeItem(String name) {
		// TODO: implement
	}
}