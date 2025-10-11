using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Collections.Generic;
using SharpPipe;
using System.Linq;
using Awwdio;
using Obsidian.Elements;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class ReverbProxy : AudioProcessorNode1ProxyBase
    {
        public ZitaParameters parameters;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        private ZitaParameters defaultParameters = new ZitaParameters();

        public ReverbController _controller = new();

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

                if (parameters.Equals(defaultParameters)) // if nothing is plugged into parameters, use the default output of the ConstructZitaParameters node
                {
                    parameters = new ZitaParameters
                    {
                        InDelay = 0,
                        Crossover = 200,
                        RT60Low = 1.49f,
                        RT60Mid = 1.2f,
                        HighFrequencyDamping = 6000,
                        EQ1Frequency = 250,
                        EQ1Level = 0,
                        EQ2Frequency = 5000,
                        EQ2Level = 0,
                        Mix = 0.7f,
                        Level = 8
                    };
                }

                AudioInput.Read(buffer, simulator);

                _controller.Process(buffer, parameters);
            }
            
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                lock (_controller)
                {
                    foreach (var key in _controller.reverbTypes)
                    {
                        _controller.updateBools[key] = true;
                    }
                }
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Effects")]
    public class Reverb : AudioProcessorNode1Base<ReverbProxy>
    {
        [ChangeListener]
        public readonly ValueInput<ZitaParameters> Parameters;

        private ZitaParameters lastParameters;

        public override void Changed(FrooxEngineContext context)
        {
            ReverbProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return;
            }
            base.Changed(context);
            proxy.parameters = Parameters.Evaluate(context);
            if (!proxy.parameters.Equals(lastParameters))
            {
                lock (proxy._controller)
                    proxy._controller.Clear();
            }
            lastParameters = proxy.parameters;
        }
    }
}