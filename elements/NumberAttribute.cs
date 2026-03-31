namespace sumstories.elements;

internal class NumberAttribute: IAttribute {
	public string Name { get; set; }
	public int Value { get; set; } = 0;
	public string Unit { get; set; } = "";
	private Accuracy accuracy = Accuracy.Exact;
	public Accuracy Accuracy {
		get => accuracy;
		set {
			if (value == Accuracy.Exact || value == Accuracy.Approximate) {
				MaxValue = null;
			} else {
				MaxValue = Value;
			}
			accuracy = value;
		}
	}
	public int? MaxValue { get; set; }

	public NumberAttribute(string Name) => this.Name = Name;

	public NumberAttribute(string Name, string Unit) {
		this.Name = Name;
		this.Unit = Unit;
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

	public override string ToString() {
		if (Accuracy == Accuracy.Range) {
			return $"{Name}: {Value} - {MaxValue} {Unit}";
		} else {
			return $"{Name}: {(Accuracy == Accuracy.Approximate ? "~" : "")}{Value} {Unit}";
		}
	}
}