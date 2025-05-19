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
                var masterIndex = DroneRecycler.equipmentDroneIndex;
                foreach (var minion in minionGroup.members)
                {
                    if (minion && minion.gameObject && minion.gameObject.TryGetComponent<CharacterMaster>(out var minionMaster) &&
                        minionMaster.masterIndex == masterIndex &&
                        minionMaster.inventory.currentEquipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
                    {
                        return minionMaster;
                    }
                }
            }
            return null;
        }
    }
}