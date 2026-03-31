namespace sumstories.elements.timeline;

internal class Date {
	public int Day { get; set; }
	public int Month { get; set; }
	public int Year { get; set; }
	public Era Era { get; set; }

	public void SetDate(int day, int month, int year) {
		this.Day = day;
		this.Month = month;
		this.Year = year;
	}
}