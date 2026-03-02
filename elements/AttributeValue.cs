namespace sumstories.elements;
using System;

public class AttributeValue {
	public string Name { get; set; }
	public string Text { get; set; }

	public AttributeValue(string name, string text) {
		this.Name = name;
		this.Text = text;
	}
}
