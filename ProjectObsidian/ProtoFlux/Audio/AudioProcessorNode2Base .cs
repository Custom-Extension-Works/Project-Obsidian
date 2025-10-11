using System;
using Awwdio;
using Elements.Assets;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public abstract class AudioProcessorNode2ProxyBase : AudioProcessorNode1ProxyBase
    {
        public IWorldAudioDataSource AudioInput2;
    }
    [NodeCategory("Obsidian/Audio")]
    public abstract class AudioProcessorNode2Base<P> : AudioProcessorNode1Base<P> where P : AudioProcessorNode2ProxyBase, new()
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> AudioInput2;

        public override void Changed(FrooxEngineContext context)
        {
            P proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.AudioInput2 = AudioInput2.Evaluate(context);
        }
    }
}