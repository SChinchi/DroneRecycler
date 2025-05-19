using RoR2;
using UnityEngine.Networking;

namespace DroneRecycler;

internal class EquipmentDroneRecyclerController : NetworkBehaviour
{
    private CharacterMaster drone;
    private GenericPickupController pickup;

    private void Awake()
    {
        if (!NetworkServer.active)
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!drone)
        {
            enabled = false;
            return;
        }
        if (!pickup || pickup.NetworkRecycled)
        {
            foreach (var ai in drone.AiComponents)
            {
                ai.customTarget.gameObject = null;
            }
            enabled = false;
        }
    }

    internal void SetTarget(CharacterMaster droneMaster, GenericPickupController pickup)
    {
        if (!Util.HasEffectiveAuthority(netIdentity))
        {
            return;
        }
        NetworkInstanceId droneNetId = droneMaster ? droneMaster.netId : NetworkInstanceId.Invalid;
        NetworkInstanceId pickupNetId = pickup ? pickup.netId : NetworkInstanceId.Invalid;
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
        foreach (var ai in drone.aiComponents)
        {
            ai.customTarget.gameObject = pickup?.gameObject;
        }
        enabled = drone && pickup;
    }
}