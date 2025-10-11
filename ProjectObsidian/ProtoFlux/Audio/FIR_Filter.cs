using System;
using System.Collections.Generic;
using System.Linq;
using Awwdio;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using Obsidian.Elements;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    public class FIR_FilterProxy : AudioProcessorNode1ProxyBase
    {
        public readonly SyncFieldList<float> Coefficients;

        public override int ChannelCount => AudioInput?.ChannelCount ?? 0;

        public FIR_FilterController _controller = new();

        protected override void OnAwake()
        {
            Coefficients.Changed += OnChanged;
            Coefficients.ElementsAdded += OnElementsAddedOrRemoved;
            Coefficients.ElementsRemoved += OnElementsAddedOrRemoved;
            base.OnAwake();
        }

        public void OnChanged(IChangeable changeable)
        {
            float[] coeffs = null;
            if (!Coefficients.IsDisposed)
            {
                lock (_controller)
                {
                    if (_controller.Coefficients != null && _controller.Coefficients.Length == Coefficients.Count)
                    {
                        coeffs = _controller.Coefficients;
                        for (int i = 0; i < Coefficients.Count; i++)
                        {
                            coeffs[i] = Coefficients[i];
                        }
                    }
                    else
                    {
                        coeffs = Coefficients.ToArray();
                    }
                    _controller.Coefficients = coeffs;
                    foreach (var filter in _controller.filters.Values)
                    {
                        ((IFirFilter)filter).SetCoefficients(coeffs);
                    }
                }
            }
        }

        public void OnElementsAddedOrRemoved(SyncElementList<Sync<float>> list, int startIndex, int count)
        {
            lock (_controller)
                _controller.Clear();
        }

        public override void Read<S>(Span<S> buffer, AudioSimulator simulator)
        {
            lock (_controller)
            {
                if (AudioInput == null)
                {
                    _controller.Clear();
                }
                if (!IsActive || AudioInput == null || !AudioInput.IsActive || _controller.Coefficients == null || _controller.Coefficients.Length == 0)
                {
                    buffer.Fill(default(S));
                    return;
                }
                AudioInput.Read(buffer, simulator);
                _controller.Process(buffer);
            }
        }

        protected override void OnStart()
        {
            Engine.AudioSystem.AudioUpdate += () =>
            {
                lock (_controller)
                {
                    foreach (var key in _controller.filterTypes)
                    {
                        _controller.updateBools[key] = true;
                    }
                }
            };
        }
    }
    [NodeCategory("Obsidian/Audio/Filters")]
    public class FIR_Filter : AudioProcessorNode1Base<FIR_FilterProxy>
    {
        public readonly ValueInput<int> CoefficientIndex;

        public readonly ValueInput<float> CoefficientValue;

        [PossibleContinuations(new string[] { "OnSetCoefficient" })]
        public readonly Operation SetCoefficient;

        [PossibleContinuations(new string[] { "OnClearCoefficients" })]
        public readonly Operation ClearCoefficients;

        public Continuation OnSetCoefficient;

        public Continuation OnClearCoefficients;

        private IOperation DoSetCoefficient(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            var index = CoefficientIndex.Evaluate(context);
            if (index < 0) return null;
            float value = CoefficientValue.Evaluate(context);
            int prevCount = proxy.Coefficients.Count;
            proxy.Coefficients.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsAdded -= proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.EnsureMinimumCount(index + 1);
            proxy.Coefficients.ElementsAdded += proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients[index] = value;
            proxy.Coefficients.Changed += proxy.OnChanged;
            if (prevCount != proxy.Coefficients.Count)
            {
                lock (proxy._controller)
                    proxy._controller.Clear();
            }
            else
            {
                float[] coeffs;
                lock (proxy._controller)
                {
                    if (proxy._controller.Coefficients != null && proxy._controller.Coefficients.Length == proxy.Coefficients.Count)
                    {
                        coeffs = proxy._controller.Coefficients;
                        for (int i = 0; i < proxy.Coefficients.Count; i++)
                        {
                            coeffs[i] = proxy.Coefficients[i];
                        }
                    }
                    else
                    {
                        coeffs = proxy.Coefficients.ToArray();
                    }
                    proxy._controller.Coefficients = coeffs;
                    foreach (var filter in proxy._controller.filters.Values)
                    {
                        ((IFirFilter)filter).SetCoefficients(coeffs);
                    }
                }
            }
            return OnSetCoefficient.Target;
        }

        private IOperation DoClearCoefficients(FrooxEngineContext context)
        {
            FIR_FilterProxy proxy = GetProxy(context);
            if (proxy == null)
            {
                return null;
            }
            proxy.Coefficients.Changed -= proxy.OnChanged;
            proxy.Coefficients.ElementsRemoved -= proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.Clear();
            proxy.Coefficients.ElementsRemoved += proxy.OnElementsAddedOrRemoved;
            proxy.Coefficients.Changed += proxy.OnChanged;
            lock (proxy._controller)
                proxy._controller.Clear();
            return OnClearCoefficients.Target;
        }

        public FIR_Filter()
        {
            SetCoefficient = new Operation(this, 0);
            ClearCoefficients = new Operation(this, 1);
        }
    }
}