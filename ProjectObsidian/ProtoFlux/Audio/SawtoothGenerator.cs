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
    public class SawtoothGeneratorProxy : AudioGeneratorNodeProxyBase
    {
        public float Frequency;

        public float Amplitude;

        public float Phase;

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
            temptime %= (1f / Frequency);
            var clampedAmplitude = MathX.Clamp01(Amplitude);
            float advance = (1f / (float)simulator.SampleRate);
            for (int i = 0; i < buffer.Length; i++)
            {
                tempBuffer[i] = (2.0f * ((((float)temptime / (1f / Frequency)) + Phase) % 1.0f) - 1.0f) * clampedAmplitude;
                if (tempBuffer[i] > 1f) tempBuffer[i] = 1f;
                else if (tempBuffer[i] < -1f) tempBuffer[i] = -1f;
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
    public class SawtoothGenerator : AudioGeneratorNodeBase<SawtoothGeneratorProxy>
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

        [PossibleContinuations(new string[] { "OnReset" })]
        public readonly Operation Reset;

        public Continuation OnReset;

        public override void Changed(FrooxEngineContext context)
        {
            SawtoothGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.Amplitude = Amplitude.Evaluate(context, 1f);
            proxy.Phase = Phase.Evaluate(context, 0f);
            proxy.Frequency = Frequency.Evaluate(context, 440f);
        }

        private IOperation DoReset(FrooxEngineContext context)
        {
            SawtoothGeneratorProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.time = 0f;
            return OnReset.Target;
        }

        public SawtoothGenerator()
        {
            Reset = new Operation(this, 0);
        }
    }
}