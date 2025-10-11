using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class ChannelSplitterProxy : AudioProcessorNode1ProxyBase
    {
        public int Channel;

        public override int ChannelCount => 1;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            if (AudioInput == null || AudioInput.ChannelCount < Channel + 1 || Channel < 0)
            {
                buffer.Fill(default(S));
                return;
            }

            switch (AudioInput.ChannelCount)
            {
                case 1:
                    Span<MonoSample> monoBuf = stackalloc MonoSample[buffer.Length];
                    AudioInput.Read(monoBuf, simulator);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, monoBuf[i][Channel]);
                    }
                    break;
                case 2:
                    Span<StereoSample> stereoBuf = stackalloc StereoSample[buffer.Length];
                    AudioInput.Read(stereoBuf, simulator);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, stereoBuf[i][Channel]);
                    }
                    break;
                case 4:
                    Span<QuadSample> quadBuf = stackalloc QuadSample[buffer.Length];
                    AudioInput.Read(quadBuf, simulator);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, quadBuf[i][Channel]);
                    }
                    break;
                case 6:
                    Span<Surround51Sample> surroundBuf = stackalloc Surround51Sample[buffer.Length];
                    AudioInput.Read(surroundBuf, simulator);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = buffer[i].SetChannel(0, surroundBuf[i][Channel]);
                    }
                    break;
            }
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class ChannelSplitter : AudioProcessorNode1Base<ChannelSplitterProxy>
    {
        [ChangeListener]
        public readonly ValueInput<int> Channel;

        public override void Changed(FrooxEngineContext context)
        {
            ChannelSplitterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.Channel = Channel.Evaluate(context);
        }
    }
}