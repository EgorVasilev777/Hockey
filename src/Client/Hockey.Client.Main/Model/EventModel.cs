using Hockey.Client.Main.Events;
using Hockey.Client.Main.Model.Abstraction;
using Hockey.Client.Main.Model.Data;
using Hockey.Client.Main.Model.Data.Events;
using Hockey.Client.Shared.Extensions;
using Prism.Events;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hockey.Client.Main.Model;

internal class EventModel : ReactiveObject, IEventModel
{
    [Reactive] public ObservableCollection<EventInfo> Events { get; set; }
    [Reactive] public ObservableCollection<IEventFactory> EventFactories { get; set; }
    [Reactive] public IEnumerable<TeamInfo> Teams { get; set; }
    public IGameStore Store { get; }
    public IEventAggregator EventAggregator { get; }

    public EventModel(IGameStore store, IEventAggregator eventAggregator)
    {
        Store = store;
        EventAggregator = eventAggregator;
        Store.WhenAnyValue(x => x.Events)
             .Subscribe(x => Events = x)
             .Cache();

        Store.WhenAnyValue(x => x.EventFactories)
             .Subscribe(x => EventFactories = x)
             .Cache();

        Store.WhenAnyValue(x => x.HomeTeam,
                           x => x.GuestTeam,
                           (home, guest) => new[] { home, guest })
            .Subscribe(x => Teams = x)
            .Cache();
    }

    public EventInfo CreateEvent(DefaultEventFactory factory)
    {
        var eventInfo = factory.Create();
        eventInfo.MillisecondsPerFrame = Store.MillisecondsPerFrame;

        eventInfo.StartEventFrameNumber = Store.FrameNumber;
        eventInfo.EndEventFrameNumber = eventInfo.StartEventFrameNumber + (long)(eventInfo.DefaultDuration.TotalMilliseconds / eventInfo.MillisecondsPerFrame);

        return eventInfo;
    }

    public void PlayEvent(EventInfo eventInfo)
    {
        EventAggregator.GetEvent<PlayEvent>().Publish(eventInfo);
    }
}
