using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class TextAttribute: IAttribute {
	public string Name { get; set; }
	public string Value { get; set; } = "";

	public TextAttribute(string Name) => this.Name = Name;

	public TextAttribute(string Name, string Value) {
		this.Name = Name;
		this.Value = Value;
	}

	public override string ToString() {
		return $"{Name}: {Value}";
	}

	/// <summary>
	/// Creates a deep copy of this text attribute.
	/// </summary>
	/// <returns></returns>
	public IAttribute Clone() {
		return new TextAttribute(this.Name, this.Value);
	}
}
