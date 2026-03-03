using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeName("Floor To Interval")]
    [NodeCategory("Obsidian/Math")]
    public class FloorToInterval_Double : ValueFunctionNode<FrooxEngineContext, double>
    {
        public ValueInput<double> Value;
        public ValueInput<double> Interval;

        protected override double Compute(FrooxEngineContext context)
        {
            double interval = Interval.Evaluate(context);
            return MathX.Floor(Value.Evaluate(context) / interval) * interval;
        }
    }
}
