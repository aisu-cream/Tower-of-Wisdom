using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An object that creates timeline of events and invokes them as the timeline progresses
/// </summary>
public class Timeline {

    readonly List<TimelineEvent> events;

    float currentTime = 0;     // the current time
    int currentIndex = 0;      // the index of the first unplayed event

    bool isTicking;

    /// <summary>
    /// Creates a timeline that executes registered events in chronological order.
    /// </summary>
    /// <remarks>
    /// Events are sorted by their sheduled time before execution. When multiple events
    /// have the same timestamp, their original ordering in the provided collection is preserved. <br/><br/>
    /// 
    /// Timeline events are executed when the timeline is advanced through <see cref="Tick"/>.
    /// Event callbacks must not call <see cref="Tick"/> or <see cref="Reset"/> on this timeline,
    /// as modifying the timeline while it is executing events results in undefined behavior.
    /// Lifecycle control such as resetting, restarting, or cancelling the timeline should be
    /// managed by the owning system.
    /// </remarks>
    /// <param name="events">The collection of events to execute. Events are ordered by their scheduled time.</param>
    /// <exception cref="ArgumentNullException">Thrown when <param name="events"/> is null.</exception>
    public Timeline(IEnumerable<TimelineEvent> events) {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        this.events = events.Where(x => x != null).OrderBy(x => x.Time).ToList();
    }

    /// <summary>
    /// Checks if the timeline progress reached the end
    /// </summary>
    /// <returns>
    ///     true, if terminated, <br/>
    ///     false, otherwise
    /// </returns>
    public bool IsFinished() {
        return currentIndex >= events.Count;
    }

    /// <summary>
    /// Reset the timeline progress
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this method is called by the callback of <see cref="Tick"/>.</exception>
    public void Reset() {
        if (isTicking)
            throw new InvalidOperationException("Cannot modify Timeline while executing callbacks.");

        currentTime = 0;
        currentIndex = 0;
    }

    /// <summary>
    /// Progresses the timeline by deltaTime and invokes every event between the previous time and the new time (inclusive).
    /// </summary>
    /// <param name="deltaTime">The amount of time to progress the timeline</param>
    /// <exception cref="InvalidOperationException">Thrown when this method is called by the callback of <see cref="Tick"/>.</exception>
    public void Tick(float deltaTime) {
        if (isTicking)
            throw new InvalidOperationException("Cannot modify Timeline while executing callbacks.");

        float newTime = currentTime + deltaTime;
        int newIndex = currentIndex;

        isTicking = true;

        while (newIndex < events.Count && events[newIndex].Time <= newTime)
            events[newIndex++].Invoke();

        isTicking = false;

        currentTime = newTime;
        currentIndex = newIndex;
    }
}

/// <summary>
/// Event with time and action callback, primarily used for <see cref="Timeline"/>.
/// </summary>
public class TimelineEvent {

    public float Time { get; }
    readonly Action Action;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineEvent"/> class with the specified execution time and action.
    /// </summary>
    /// <param name="time">The time, in seconds, at which the event is executed. Must be non-negative.</param>
    /// <param name="action">The callback to invoke when the event is executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="time"/> is negative.
    /// </exception>
    public TimelineEvent(float time, Action action) {
        if (time < 0)
            throw new ArgumentOutOfRangeException(nameof(time), time, "Timeline event time cannot be negative.");

        Time = time;
        Action = action;
    }

    internal void Invoke() {
        Action?.Invoke();
    }
}