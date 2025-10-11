using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Runtime.InteropServices;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class SquareGeneratorProxy : AudioGeneratorNodeProxyBase
    {
        public float Frequency;

        public float Amplitude;

        public float Phase;

        public float PulseWidth;

        public double time;

        private float[] tempBuffer = null;

        public override int ChannelCount => 1;

        private bool updateTime;

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            if (!updateTime && tempBuffer != null)
            {
                double position2 = 0.0;
                MonoSample lastSample2 = default(MonoSample);
                MemoryMarshal.Cast<float, MonoSample>(MemoryExtensions.AsSpan(tempBuffer)).CopySamples<MonoSample, S>(buffer, ref position2, ref lastSample2, 1.0);
                return;
            }

            tempBuffer = tempBuffer.EnsureSize(buffer.Length);
            var temptime = time;
            float period = (1f / Frequency);
            temptime %= period;
            var clampedAmplitude = MathX.Clamp01(Amplitude);
            float advance = (1f / (float)simulator.SampleRate);
            
            for (int i = 0; i < buffer.Length; i++)
            {
                if ((temptime + (Phase * period)) % period <= PulseWidth / Frequency)
                {
                    tempBuffer[i] = 1f * clampedAmplitude;
                }
                else
                {
                    tempBuffer[i] = -1f * clampedAmplitude;
                }
                temptime += advance;
            }
            if (updateTime)
            {
                time = temptime;
                updateTime = false;
            }
            double position = 0.0;
            MonoSample lastSample = default(MonoSample);
            MemoryMarshal.Cast<float, MonoSample>(MemoryExtensions.AsSpan(tempBuffer)).CopySamples(buffer, ref position, ref lastSample);
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                updateTime = true;
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Generators")]
    public class SquareGenerator : AudioGeneratorNodeBase<SquareGeneratorProxy>
    {
        [ChangeListener]
        [DefaultValueAttribute(440f)]
        public readonly ValueInput<float> Frequency;

        [ChangeListener]
        [DefaultValueAttribute(1f)]
        public readonly ValueInput<float> Amplitude;

        [ChangeListener]
        [DefaultValueAttribute(0f)]
        public readonly ValueInput<float> Phase;

        [ChangeListener]
        [DefaultValueAttribute(0.5f)]
        public readonly ValueInput<float> PulseWidth;

        [PossibleContinuations(new string[] { "OnReset" })]
        public readonly Operation Reset;

        public Continuation OnReset;

        public override void Changed(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.Amplitude = Amplitude.Evaluate(context, 1f);
            proxy.Phase = Phase.Evaluate(context, 0f);
            proxy.Frequency = Frequency.Evaluate(context, 440f);
            proxy.PulseWidth = PulseWidth.Evaluate(context, 0.5f);
        }

        private IOperation DoReset(FrooxEngineContext context)
        {
            SquareGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.time = 0f;
            return OnReset.Target;
        }

        public SquareGenerator()
        {
            Reset = new Operation(this, 0);
        }
    }
}