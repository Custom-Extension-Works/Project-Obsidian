using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class StereoCombinerProxy : AudioGeneratorNodeProxyBase
    {
        public IWorldAudioDataSource Left;

        public IWorldAudioDataSource Right;

        public override int ChannelCount => 2;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<StereoSample> samples = stackalloc StereoSample[buffer.Length];
            Span<MonoSample> newBuffer = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer2 = stackalloc MonoSample[buffer.Length];
            samples.Fill(default);
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            if (Left != null && Left.ChannelCount == 1)
            {
                Left.Read(newBuffer, simulator);
            }
            if (Right != null && Right.ChannelCount == 1)
            {
                Right.Read(newBuffer2, simulator);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i] = samples[i].SetChannel(0, newBuffer[i][0]);
                samples[i] = samples[i].SetChannel(1, newBuffer2[i][0]);
            }

            double position = 0.0;
            StereoSample lastSample = default(StereoSample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class StereoCombiner : AudioGeneratorNodeBase<StereoCombinerProxy>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> Left;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> Right;

        public override void Changed(FrooxEngineContext context)
        {
            StereoCombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.Left = Left.Evaluate(context);
            proxy.Right = Right.Evaluate(context);
        }
    }
}