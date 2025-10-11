using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioAdderProxy : AudioProcessorNode2ProxyBase
    {
        public override int ChannelCount => AudioInput?.ChannelCount ?? AudioInput2?.ChannelCount ?? 0;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
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
                
            Span<S> buffer2s = stackalloc S[buffer.Length];
            buffer2s.Fill(default);
            if (AudioInput2 != null)
            {
                AudioInput2.Read(buffer2s, simulator);
            }
                
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = buffer1s[i].Add(buffer2s[i]);
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class AudioAdder : AudioProcessorNode2Base<AudioAdderProxy>
    {
    }
}