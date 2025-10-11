using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class QuadCombinerProxy : AudioGeneratorNodeProxyBase
    {
        public IWorldAudioDataSource LeftFront;

        public IWorldAudioDataSource RightFront;

        public IWorldAudioDataSource LeftRear;

        public IWorldAudioDataSource RightRear;

        public override int ChannelCount => 4;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<QuadSample> samples = stackalloc QuadSample[buffer.Length];
            Span<MonoSample> newBuffer = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer2 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer3 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer4 = stackalloc MonoSample[buffer.Length];
            samples.Fill(default);
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            newBuffer3.Fill(default);
            newBuffer4.Fill(default);
            if (LeftFront != null && LeftFront.ChannelCount == 1)
            {
                LeftFront.Read(newBuffer, simulator);
            }
            if (RightFront != null && RightFront.ChannelCount == 1)
            {
                RightFront.Read(newBuffer2, simulator);
            }
            if (LeftRear != null && LeftRear.ChannelCount == 1)
            {
                LeftRear.Read(newBuffer3, simulator);
            }
            if (RightRear != null && RightRear.ChannelCount == 1)
            {
                RightRear.Read(newBuffer4, simulator);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i] = samples[i].SetChannel(0, newBuffer[i][0]);
                samples[i] = samples[i].SetChannel(1, newBuffer2[i][0]);
                samples[i] = samples[i].SetChannel(2, newBuffer3[i][0]);
                samples[i] = samples[i].SetChannel(3, newBuffer4[i][0]);
            }

            double position = 0.0;
            QuadSample lastSample = default(QuadSample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class QuadCombiner : AudioGeneratorNodeBase<QuadCombinerProxy>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> LeftFront;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> RightFront;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> LeftRear;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> RightRear;

        public override void Changed(FrooxEngineContext context)
        {
            QuadCombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.LeftFront = LeftFront.Evaluate(context);
            proxy.RightFront = RightFront.Evaluate(context);
            proxy.LeftRear = LeftRear.Evaluate(context);
            proxy.RightRear = RightRear.Evaluate(context);
        }
    }
}