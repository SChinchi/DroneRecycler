using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace DroneRecycler;

internal class Hooks
{
    internal static GenericPickupController pingedTarget;

    public static void Init()
    {
        On.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition += IsTargetPickup;
        On.RoR2.PingerController.RebuildPing += CommandEquipmentDroneWithPing;
    }

    private static bool IsTargetPickup(On.RoR2.CharacterAI.BaseAI.Target.orig_GetBullseyePosition orig, BaseAI.Target self, out Vector3 position)
    {
        var result = orig(self, out position);
        result |= self.gameObject != null && self.gameObject.GetComponent<GenericPickupController>() != null;
        return result;
    }

    private static void CommandEquipmentDroneWithPing(On.RoR2.PingerController.orig_RebuildPing orig, PingerController self, PingerController.PingInfo pingInfo)
    {
        orig(self, pingInfo);
        var pickup = self.pingIndicator?.pingTarget?.GetComponent<GenericPickupController>();
        pingedTarget = pickup;
        if (pickup != null && !pickup.Recycled && !DroneRecycler.isCommandManual.Value)
        {
            var droneMaster = Utils.FindEquipmentDroneWithRecycler(self.gameObject);
            if (droneMaster != null)
            {
                self.GetComponent<EquipmentDroneRecyclerController>().SetTarget(droneMaster, pickup);
            }
        }
    }
}