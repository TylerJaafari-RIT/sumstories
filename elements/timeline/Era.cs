namespace sumstories.elements.timeline;

// EACH ERA SHOULD HAVE ITS OWN ARTICLE
internal class Era {
	string Name { get; set; }
	string Description { get; set; }
	Timeline Timeline { get; set; }
	int StartYear { get; set; }
	int EndYear { get; set; }
}