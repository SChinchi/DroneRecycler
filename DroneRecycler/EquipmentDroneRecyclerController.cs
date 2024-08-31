using RoR2;
using RoR2.CharacterAI;
using UnityEngine.Networking;

namespace DroneRecycler
{
    internal class EquipmentDroneRecyclerController : NetworkBehaviour
    {
        private CharacterMaster drone;
        private GenericPickupController pickup;

        public void SetTarget(CharacterMaster droneMaster, GenericPickupController pickup)
        {
            if (!Util.HasEffectiveAuthority(netIdentity))
            {
                return;
            }
            NetworkInstanceId droneNetId = droneMaster != null ? droneMaster.netId : NetworkInstanceId.Invalid;
            NetworkInstanceId pickupNetId = pickup != null ? pickup.netId : NetworkInstanceId.Invalid;
            if (NetworkServer.active)
            {
                SetTargetInternal(droneNetId, pickupNetId);
                return;
            }
            CmdSetTarget(droneNetId, pickupNetId);
        }

        [Command]
        private void CmdSetTarget(NetworkInstanceId droneNetId, NetworkInstanceId pickupNetId)
        {
            SetTargetInternal(droneNetId, pickupNetId);
        }

        private void SetTargetInternal(NetworkInstanceId droneNetId, NetworkInstanceId pickupNetId)
        {
            pickup = Util.FindNetworkObject(pickupNetId)?.GetComponent<GenericPickupController>();
            drone = Util.FindNetworkObject(droneNetId)?.GetComponent<CharacterMaster>();
            drone.GetComponent<BaseAI>().customTarget.gameObject = pickup?.gameObject;
            enabled = drone && pickup;
        }

        private void Awake()
        {
            if (!NetworkServer.active)
            {
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (drone == null)
            {
                enabled = false;
                return;
            }
            if (pickup == null || pickup.NetworkRecycled)
            {
                drone.GetComponent<BaseAI>().customTarget.gameObject = null;
                enabled = false;
            }
        }
    }
}