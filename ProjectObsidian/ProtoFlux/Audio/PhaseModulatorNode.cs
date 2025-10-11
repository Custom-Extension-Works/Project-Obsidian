using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using Obsidian.Elements;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class PhaseModulatorProxy : AudioProcessorNode2ProxyBase
    {
        public float ModulationIndex;

        public override int ChannelCount => MathX.Min(AudioInput?.ChannelCount ?? 0, AudioInput2?.ChannelCount ?? 0);

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive || AudioInput == null || AudioInput2 == null)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<S> newBuffer = stackalloc S[buffer.Length];
            Span<S> newBuffer2 = stackalloc S[buffer.Length];
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            AudioInput.Read(newBuffer, simulator);
            AudioInput2.Read(newBuffer2, simulator);

            Algorithms.PhaseModulation(buffer, newBuffer, newBuffer2, ModulationIndex, ChannelCount);
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class PhaseModulator : AudioProcessorNode2Base<PhaseModulatorProxy>
    {
        [ChangeListener]
        public readonly ValueInput<float> ModulationIndex;

        public override void Changed(FrooxEngineContext context)
        {
            PhaseModulatorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.ModulationIndex = ModulationIndex.Evaluate(context);
        }
    }
}