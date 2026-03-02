using System;
using System.Collections.Generic;
using System.Text;

namespace sumstories.elements.timeline;

internal class Timeline {
	string Name { get; set; }
	List<Era> Eras { get; }
	List<TimelineEvent> Events { get; }

	public Timeline() {

	}

	public void AddEvent(TimelineEvent tEvent) {
		Events.Add(tEvent);
		tEvent.Timeline = this;
	}

	public TimelineEvent AddEvent() {
		TimelineEvent tEvent = new TimelineEvent();
		tEvent.Timeline = this;
		// TODO: frontend should redirect to newly created event page
		return new TimelineEvent();
	}


}
