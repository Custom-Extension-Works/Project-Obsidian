using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeName("Ceil To Interval")]
    [NodeCategory("Obsidian/Math")]
    public class CeilToInterval_Float : ValueFunctionNode<FrooxEngineContext, float>
    {
        public ValueInput<float> Value;
        public ValueInput<float> Interval;

        protected override float Compute(FrooxEngineContext context)
        {
            float interval = Interval.Evaluate(context);
            return MathX.Ceil(Value.Evaluate(context) / interval) * interval;
        }
    }
}
