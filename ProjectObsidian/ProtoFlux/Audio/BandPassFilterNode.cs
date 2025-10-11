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
    public class BandPassFilterProxy : AudioProcessorNode1ProxyBase
    {
        public float LowFrequency;

        public float HighFrequency;

        public float Resonance;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        private BandPassFilterController _controller = new();

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            lock (_controller)
            {
                if (AudioInput == null)
                {
                    _controller.Clear();
                }
                if (!IsActive || AudioInput == null)
                {
                    buffer.Fill(default(S));
                    return;
                }

                AudioInput.Read(buffer, simulator);

                _controller.Process(buffer, simulator.SampleRate, LowFrequency, HighFrequency, Resonance);
            }
            
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class BandPassFilter : AudioProcessorNode1Base<BandPassFilterProxy>
    {
        [ChangeListener]
        [DefaultValueAttribute(20f)]
        public readonly ValueInput<float> LowFrequency;

        [ChangeListener]
        [DefaultValueAttribute(20000f)]
        public readonly ValueInput<float> HighFrequency;

        [ChangeListener]
        [DefaultValueAttribute(1.41f)]
        public readonly ValueInput<float> Resonance;

        public override void Changed(FrooxEngineContext context)
        {
            BandPassFilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.LowFrequency = LowFrequency.Evaluate(context, 20f);
            proxy.HighFrequency = HighFrequency.Evaluate(context, 20000f);
            proxy.Resonance = Resonance.Evaluate(context, 1.41f);
        }
    }
}