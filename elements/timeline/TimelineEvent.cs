namespace sumstories.elements.timeline;

internal class TimelineEvent {
	public string Name { get; set; }
	public string Description { get; set; }
	public Timeline Timeline { get; set; }
	public Date Date { get; set; }

	public void AddToTimeline(Timeline timeline) {
		timeline.AddEvent(this);
	}
}