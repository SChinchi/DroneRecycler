using RoR2;
using UnityEngine;

namespace DroneRecycler
{
    internal class Utils
    {
        public static CharacterMaster FindEquipmentDroneWithRecycler(GameObject masterObject)
        {
            var master = masterObject.GetComponent<CharacterMaster>();
            var minionGroup = MinionOwnership.MinionGroup.FindGroup(master.netId);
            if (minionGroup != null)
            {
                foreach (var minion in minionGroup.members)
                {
                    if (minion.gameObject.name.Contains("EquipmentDrone"))
                    {
                        var minionMaster = minion.GetComponent<CharacterMaster>();
                        if (minionMaster.inventory.currentEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
                        {
                            return minionMaster;
                        }
                    }
                }
            }
            return null;
        }
    }
}