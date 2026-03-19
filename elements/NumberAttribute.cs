namespace sumstories.elements;

internal class NumberAttribute : IAttribute {
	public string Name { get; set; }
	public int Value { get; set; }
	public Accuracy Accuracy { get;
		set {
			if(value == Accuracy.Exact || value == Accuracy.Approximate) {
				MaxValue = null;
			} else {
				MaxValue = Value;
			}
		}
	} = Accuracy.Exact;
	public int? MaxValue { get; set; }

	public NumberAttribute(string Name) {
		this.Name = Name;
		this.Value = 0;
	}

	public NumberAttribute(string Name, int Value) {
		this.Name = Name;
		this.Value = Value;
	}

	public NumberAttribute(string Name, int Min, int Max) {
		this.Name = Name;
		this.Value = Min;
		this.MaxValue = Max;
		this.Accuracy = Accuracy.Range;
	}
}