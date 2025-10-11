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
    public class ButterworthFilterProxy : AudioProcessorNode1ProxyBase
    {
        public bool LowPass;

        public float Frequency;

        public float Resonance;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        private ButterworthFilterController _controller = new();

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

                _controller.Process(buffer, simulator.SampleRate, LowPass, Frequency, Resonance);
            }
            
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class ButterworthFilter : AudioProcessorNode1Base<ButterworthFilterProxy>
    {
        [ChangeListener]
        public readonly ValueInput<bool> LowPass;

        [ChangeListener]
        [DefaultValueAttribute(20f)]
        public readonly ValueInput<float> Frequency;

        [ChangeListener]
        [DefaultValueAttribute(1.41f)]
        public readonly ValueInput<float> Resonance;

        public override void Changed(FrooxEngineContext context)
        {
            ButterworthFilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.LowPass = LowPass.Evaluate(context);
            proxy.Frequency = Frequency.Evaluate(context, 20f);
            proxy.Resonance = Resonance.Evaluate(context, 1.41f);
        }
    }
}