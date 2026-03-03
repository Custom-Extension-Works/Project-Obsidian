using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Renderite.Shared;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Users.Avatar
{
    [NodeCategory("Obsidian/Avatar")]
    public class SlotToBodyNode : VoidNode<FrooxEngineContext>
    {
        public ObjectArgument<Slot> Slot;

        public readonly ValueOutput<BodyNode> Node;

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            Slot slot = 0.ReadObject<Slot>(context);
            BodyNode result = BodyNode.NONE;

            if (slot != null && !slot.IsRemoved)
            {
                BipedRig rig = slot.GetComponentInParents<BipedRig>();
                if (rig != null)
                {
                    result = rig.GetBoneType(slot);
                }
            }

            Node.Write(result, context);
        }

        public SlotToBodyNode()
        {
            Node = new ValueOutput<BodyNode>(this);
        }
    }
}


