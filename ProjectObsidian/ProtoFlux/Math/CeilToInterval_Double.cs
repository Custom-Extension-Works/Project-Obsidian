using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeName("Ceil To Interval")]
    [NodeCategory("Obsidian/Math")]
    public class CeilToInterval_Double : ValueFunctionNode<FrooxEngineContext, double>
    {
        public ValueInput<double> Value;
        public ValueInput<double> Interval;

        protected override double Compute(FrooxEngineContext context)
        {
            double interval = Interval.Evaluate(context);
            return MathX.Ceil(Value.Evaluate(context) / interval) * interval;
        }
    }
}
