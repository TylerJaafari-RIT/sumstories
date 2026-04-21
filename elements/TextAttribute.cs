using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements;

internal class TextAttribute: IAttribute {
	public long ID { get; }
	public string Name { get; set; }
	public string Value { get; set; } = "";

	public TextAttribute(string Name) {
		this.ID = ID;
		this.Name = Name;
	}

	public TextAttribute(long ID, string Name, string Value) {
		this.ID = ID;
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
	public IAttribute Clone(long ID) {
		return new TextAttribute(ID, this.Name, this.Value);
	}
}
