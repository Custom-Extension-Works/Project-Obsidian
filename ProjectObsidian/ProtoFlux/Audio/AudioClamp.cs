using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioClampProxy : AudioProcessorNode1ProxyBase
    {
        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive || AudioInput == null || !AudioInput.IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            AudioInput.Read(buffer, simulator);

            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 0; j < ChannelCount; j++)
                {
                    if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                    else if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
                }
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class AudioClamp : AudioProcessorNode1Base<AudioClampProxy>
    {
    }
}