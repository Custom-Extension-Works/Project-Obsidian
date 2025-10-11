using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioMultiplyProxy : AudioProcessorNode1ProxyBase
    {

        public float Value;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive || AudioInput == null)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<S> buffer1s = stackalloc S[buffer.Length];
            buffer1s.Fill(default);
            if (AudioInput != null)
            {
                AudioInput.Read(buffer1s, simulator);
            }

            //AudioInput.Read(buffer);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = buffer1s[i].Multiply(Value);
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class AudioMultiply : AudioProcessorNode1Base<AudioMultiplyProxy>
    {
        [ChangeListener]
        public readonly ValueInput<float> Value;

        public override void Changed(FrooxEngineContext context)
        {
            AudioMultiplyProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.Value = Value.Evaluate(context);
        }
    }
}