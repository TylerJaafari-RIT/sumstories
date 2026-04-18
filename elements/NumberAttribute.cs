namespace sumstories.elements;

internal class NumberAttribute: IAttribute {
	public long ID { get; }
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
	public long? MaxValue { get; set; }

	public NumberAttribute(long ID, string Name) {
		this.ID = ID;
		this.Name = Name;
	}

	public NumberAttribute(string Name, string Unit) {
		this.Name = Name;
		this.Unit = Unit;
	}

	public NumberAttribute(long ID, string Name, int Value) {
		this.ID = ID;
		this.Name = Name;
		this.Value = Value;
	}

	public NumberAttribute(string Name, int Min, long Max) {
		this.Name = Name;
		this.Value = Min;
		this.MaxValue = Max;
		this.Accuracy = Accuracy.Range;
	}

    public NumberAttribute(NumberAttribute other, long ID) {
		this.ID = ID;
		this.Name = other.Name;
		this.Value = other.Value;
		this.Unit = other.Unit;
		this.Accuracy = other.Accuracy;
		this.MaxValue = other.MaxValue;
	}

	public override string ToString() {
		if (Accuracy == Accuracy.Range) {
			return $"{Name}: {Value} - {MaxValue} {Unit}";
		} else {
			return $"{Name}: {(Accuracy == Accuracy.Approximate ? "~" : "")}{Value} {Unit}";
		}
	}

	/// <summary>
	/// Creates a deep copy of this number attribute.
	/// </summary>
	/// <returns></returns>
	public IAttribute Clone(long ID) {
		return new NumberAttribute(this, ID);
	}
}