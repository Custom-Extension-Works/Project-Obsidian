using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class Surround51_CombinerProxy : AudioGeneratorNodeProxyBase
    {
        public IWorldAudioDataSource LeftFront;

        public IWorldAudioDataSource RightFront;

        public IWorldAudioDataSource Center;

        public IWorldAudioDataSource Subwoofer;

        public IWorldAudioDataSource LeftRear;

        public IWorldAudioDataSource RightRear;

        public override int ChannelCount => 6;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<Surround51Sample> samples = stackalloc Surround51Sample[buffer.Length];
            Span<MonoSample> newBuffer = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer2 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer3 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer4 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer5 = stackalloc MonoSample[buffer.Length];
            Span<MonoSample> newBuffer6 = stackalloc MonoSample[buffer.Length];
            samples.Fill(default);
            newBuffer.Fill(default);
            newBuffer2.Fill(default);
            newBuffer3.Fill(default);
            newBuffer4.Fill(default);
            newBuffer5.Fill(default);
            newBuffer6.Fill(default);
            if (LeftFront != null && LeftFront.ChannelCount == 1)
            {
                LeftFront.Read(newBuffer, simulator);
            }
            if (RightFront != null && RightFront.ChannelCount == 1)
            {
                RightFront.Read(newBuffer2, simulator);
            }
            if (Center != null && Center.ChannelCount == 1)
            {
                Center.Read(newBuffer3, simulator);
            }
            if (Subwoofer != null && Subwoofer.ChannelCount == 1)
            {
                Subwoofer.Read(newBuffer4, simulator);
            }
            if (LeftRear != null && LeftRear.ChannelCount == 1)
            {
                LeftRear.Read(newBuffer5, simulator);
            }
            if (RightRear != null && RightRear.ChannelCount == 1)
            {
                RightRear.Read(newBuffer6, simulator);
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i] = samples[i].SetChannel(0, newBuffer[i][0]);
                samples[i] = samples[i].SetChannel(1, newBuffer2[i][0]);
                samples[i] = samples[i].SetChannel(2, newBuffer3[i][0]);
                samples[i] = samples[i].SetChannel(3, newBuffer4[i][0]);
                samples[i] = samples[i].SetChannel(4, newBuffer5[i][0]);
                samples[i] = samples[i].SetChannel(5, newBuffer6[i][0]);
            }

            double position = 0.0;
            Surround51Sample lastSample = default(Surround51Sample);
            samples.CopySamples(buffer, ref position, ref lastSample);
        }
    }
    [NodeCategory("Obsidian/Audio")]
    public class Surround51_Combiner : AudioGeneratorNodeBase<Surround51_CombinerProxy>
    {
        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> LeftFront;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> RightFront;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> Center;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> Subwoofer;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> LeftRear;

        [ChangeListener]
        public readonly ObjectInput<IWorldAudioDataSource> RightRear;

        public override void Changed(FrooxEngineContext context)
        {
            Surround51_CombinerProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.LeftFront = LeftFront.Evaluate(context);
            proxy.RightFront = RightFront.Evaluate(context);
            proxy.Center = Center.Evaluate(context);
            proxy.Subwoofer = Subwoofer.Evaluate(context);
            proxy.LeftRear = LeftRear.Evaluate(context);
            proxy.RightRear = RightRear.Evaluate(context);
        }
    }
}