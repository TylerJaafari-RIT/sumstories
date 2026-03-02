using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class TextAttribute : Attribute {
	Accuracy Accuracy { get; set; }

	public TextAttribute(string Name, AttributeValue value) {
		base(Name, value);
	}

	public TextAttribute(string Name, AttributeValue value, Accuracy accuracy) {
		object value1 = base(Name, value);
		this.Accuracy = accuracy;
	}
}
