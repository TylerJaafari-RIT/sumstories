namespace sumstories.elements;

public abstract class Attribute {
	protected string Name { get; set; }
	
	protected AttributeValue Value { get; set; }

	public Attribute(string Name, AttributeValue value) {
		this.Name = Name;
		this.Value = value;
	}

	public override string ToString() {
		return $"{Name}: {Value}";
	}
}