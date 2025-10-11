using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Obsidian.Elements;
using Elements.Core;
using System.Collections.Generic;
using System.Linq;
using Awwdio;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class AudioDelayProxy : AudioProcessorNode1ProxyBase
    {
        public int delayMilliseconds;

        public float feedback;

        public float DryWet;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public DelayController _controller = new();

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            lock (_controller)
            {
                if (AudioInput == null)
                {
                    _controller.Clear();
                }
                if (!IsActive || AudioInput == null || !AudioInput.IsActive)
                {
                    buffer.Fill(default(S));
                    return;
                }

                AudioInput.Read(buffer, simulator);

                _controller.Process(buffer, delayMilliseconds, feedback, DryWet);
            }
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                lock (_controller)
                {
                    foreach (var key in _controller.delayTypes)
                    {
                        _controller.updateBools[key] = true;
                    }
                }
            };
        }
    }
    [NodeName("Delay")]
    [NodeCategory("Obsidian/Audio/Effects")]
    public class AudioDelay : AudioProcessorNode1Base<AudioDelayProxy>
    {
        [ChangeListener]
        public readonly ValueInput<int> DelayMilliseconds;

        [ChangeListener]
        public readonly ValueInput<float> Feedback;

        [ChangeListener]
        public readonly ValueInput<float> DryWet;

        public override void Changed(FrooxEngineContext context)
        {
            AudioDelayProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.delayMilliseconds = DelayMilliseconds.Evaluate(context);
            lock (proxy._controller)
            {
                foreach (var delay in proxy._controller.delays.Values)
                {
                    ((IDelayEffect)delay).SetDelayTime(proxy.delayMilliseconds, Engine.Current.AudioSystem.SampleRate);
                }
            }
            proxy.feedback = Feedback.Evaluate(context);
            proxy.DryWet = DryWet.Evaluate(context);
        }
    }
}