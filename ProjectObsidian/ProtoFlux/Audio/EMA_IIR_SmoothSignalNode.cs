using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class EMA_IIR_SmoothSignalProxy : AudioProcessorNode1ProxyBase
    {
        public float SmoothingFactor;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive || AudioInput == null)
            {
                buffer.Fill(default(S));
                return;
            }

            AudioInput.Read(buffer, simulator);

            Algorithms.EMAIIRSmoothSignal(ref buffer, buffer.Length, SmoothingFactor);
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class EMA_IIR_SmoothSignal : AudioProcessorNode1Base<EMA_IIR_SmoothSignalProxy>
    {
        [ChangeListener]
        public readonly ValueInput<float> SmoothingFactor;

        public override void Changed(FrooxEngineContext context)
        {
            EMA_IIR_SmoothSignalProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.SmoothingFactor = SmoothingFactor.Evaluate(context);
        }
    }
}