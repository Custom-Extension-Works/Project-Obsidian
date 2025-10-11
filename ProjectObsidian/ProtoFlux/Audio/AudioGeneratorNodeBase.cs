using System;
using Awwdio;
using Elements.Assets;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio;

public abstract class AudioGeneratorNodeProxyBase : ProtoFluxEngineProxy, IWorldAudioDataSource
{
    public bool IsActive => Active;

    public abstract int ChannelCount { get; }

    public bool Active;

    public abstract void Read<S>(Span<S> buffer, AudioSimulator system) where S : unmanaged, IAudioSample<S>;
}

[NodeCategory("Obsidian/Audio")]
public abstract class AudioGeneratorNodeBase<P> : ProxyVoidNode<FrooxEngineContext, P>, IExecutionChangeListener<FrooxEngineContext> where P : AudioGeneratorNodeProxyBase, new()
{
    public readonly ObjectOutput<IWorldAudioDataSource> AudioOutput;

    private ObjectStore<Action<IChangeable>> _enabledChangedHandler;

    private ObjectStore<SlotEvent> _activeChangedHandler;

    public bool ValueListensToChanges { get; private set; }

    private bool ShouldListen(P proxy)
    {
        if (proxy.Enabled)
        {
            return proxy.Slot.IsActive;
        }
        return false;
    }

    protected override void ProxyAdded(P proxy, FrooxEngineContext context)
    {
        base.ProxyAdded(proxy, context);
        NodeContextPath path = context.CaptureContextPath();
        ProtoFluxNodeGroup group = context.Group;
        context.GetEventDispatcher(out var dispatcher);
        Action<IChangeable> enabledHandler = delegate
        {
            dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
            {
                UpdateListenerState(c);
            });
        };
        SlotEvent activeHandler = delegate
        {
            dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
            {
                UpdateListenerState(c);
            });
        };
        proxy.EnabledField.Changed += enabledHandler;
        proxy.Slot.ActiveChanged += activeHandler;
        _enabledChangedHandler.Write(enabledHandler, context);
        _activeChangedHandler.Write(activeHandler, context);
        ValueListensToChanges = ShouldListen(proxy);
        proxy.Active = ValueListensToChanges;
    }

    protected override void ProxyRemoved(P proxy, FrooxEngineContext context, bool inUseByAnotherInstance)
    {
        if (!inUseByAnotherInstance)
        {
            proxy.EnabledField.Changed -= _enabledChangedHandler.Read(context);
            proxy.Slot.ActiveChanged -= _activeChangedHandler.Read(context);
            _enabledChangedHandler.Clear(context);
            _activeChangedHandler.Clear(context);
            proxy.Active = false;
        }
    }

    protected void UpdateListenerState(FrooxEngineContext context)
    {
        P proxy = GetProxy(context);
        if (proxy != null)
        {
            bool shouldListen = ShouldListen(proxy);
            if (shouldListen != ValueListensToChanges)
            {
                ValueListensToChanges = shouldListen;
                context.Group.MarkChangeTrackingDirty();
                proxy.Active = shouldListen;
            }
        }
    }

    public virtual void Changed(FrooxEngineContext context)
    {

    }

    protected override void ComputeOutputs(FrooxEngineContext context)
    {
        P proxy = GetProxy(context);
        AudioOutput.Write(proxy, context);
    }

    public AudioGeneratorNodeBase()
    {
        AudioOutput = new ObjectOutput<IWorldAudioDataSource>(this);
    }
}