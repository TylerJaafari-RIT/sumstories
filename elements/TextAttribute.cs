using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class TextAttribute : Attribute {
	Accuracy Accuracy { get; set; }

	public TextAttribute(string Name, AttributeValue Value): base(Name, Value) {
		
	}

	public TextAttribute(string Name, AttributeValue Value, Accuracy accuracy): base(Name, Value) {
		this.Name = Name;
		this.Value = Value;
		this.Accuracy = accuracy;
	}
}
