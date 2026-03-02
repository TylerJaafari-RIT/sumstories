namespace sumstories.elements.timeline;

internal class TimelineEvent {
	string Name { get; set; }
	string Description { get; set; }
	Timeline Timeline { get; set; }
	Date Date { get; set; }

	public void AddToTimeline(Timeline timeline) {
		timeline.AddEvent(this);
	}
}