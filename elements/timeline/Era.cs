namespace sumstories.elements.timeline;

// EACH ERA SHOULD HAVE ITS OWN ARTICLE
internal class Era {
	public string Name { get; set; }
	public string Description { get; set; }
	public Timeline Timeline { get; set; }
	public int StartYear { get; set; }
	public int EndYear { get; set; }
}