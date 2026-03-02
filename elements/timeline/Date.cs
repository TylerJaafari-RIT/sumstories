namespace sumstories.elements.timeline;

internal class Date {
	int Day { get; set; }
	int Month { get; set; }
	int Year { get; set; }

	public void SetDate(int day, int month, int year) {
		this.Day = day;
		this.Month = month;
		this.Year = year;
	}
}