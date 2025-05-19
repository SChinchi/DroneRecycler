using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace DroneRecycler;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class DroneRecycler : BaseUnityPlugin
{
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Chinchi";
    public const string PluginName = "DroneRecycler";
    public const string PluginVersion = "1.0.1";

    internal static ConfigEntry<KeyCode> commandKey;
    internal static ConfigEntry<bool> isCommandManual;
    internal static MasterCatalog.MasterIndex equipmentDroneIndex = MasterCatalog.MasterIndex.none;

    public void Awake()
    {
        commandKey = Config.Bind("Key Binds", "commandKey", KeyCode.C, "Which key to use to manually issue or revoke the recycling command");
        isCommandManual = Config.Bind("Settings", "isCommandManual", false, "Whether assigning an Equipment Drone to recycle an item is done by pressing a key (see 'commandKey') after pinging a pickup or it is done automatically. If this is enabled, the 'commandKey' can further be used to revoke the command.");
        Hooks.Init();
        RoR2Application.onLoad += Patch;
    }

    void Update()
    {
        if (Run.instance && isCommandManual.Value && Input.GetKeyDown(commandKey.Value))
        {
            if (MPEventSystemManager.eventSystems.Values.Any(x => x & x.currentSelectedGameObject)
                || UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject
                || RoR2.UI.PauseScreenController.instancesList.Count > 0)
            {
                return;
            }
            var master = NetworkUser.readOnlyLocalPlayersList.FirstOrDefault()?.master;
            if (master)
            {
                var droneMaster = Utils.FindEquipmentDroneWithRecycler(master.gameObject);
                if (droneMaster)
                {
                    var currentTarget = droneMaster.GetComponent<BaseAI>().customTarget.gameObject;
                    master.GetComponent<EquipmentDroneRecyclerController>().SetTarget(droneMaster, currentTarget == Hooks.pingedTarget.gameObject ? null : Hooks.pingedTarget);
                }
            }
        }
    }

    private void Patch()
    {
        var master = MasterCatalog.GetMasterPrefab(MasterCatalog.FindMasterIndex("EquipmentDroneMaster"));
        if (master && master.TryGetComponent<CharacterMaster>(out var characterMaster))
        {
            equipmentDroneIndex = characterMaster.masterIndex;
        }
        else
        {
            Logger.LogError("EquipmentDrone not found");
            return;
        }
        PatchAI();
        PatchPlayer();
    }

    private void PatchAI()
    {
        var master = MasterCatalog.GetMasterPrefab(equipmentDroneIndex);
        if (!master)
        {
            return;
        }
        var originalSkillDrivers = master.GetComponents<AISkillDriver>();

        var component = master.AddComponent<AISkillDriver>();
        component.customName = "RecyclePickup";
        component.skillSlot = SkillSlot.None;
        component.requireEquipmentReady = true;
        component.maxDistance = 30f;
        component.activationRequiresAimConfirmation = true;
        component.moveTargetType = AISkillDriver.TargetType.Custom;
        component.aimType = AISkillDriver.AimType.AtMoveTarget;
        component.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
        component.shouldFireEquipment = true;
        component.resetCurrentEnemyOnNextDriverSelection = true;

        component = master.AddComponent<AISkillDriver>();
        component.customName = "ApproachPickup";
        component.skillSlot = SkillSlot.None;
        component.minDistance = 30f;
        component.moveTargetType = AISkillDriver.TargetType.Custom;
        component.aimType = AISkillDriver.AimType.AtMoveTarget;
        component.movementType = AISkillDriver.MovementType.ChaseMoveTarget;

        component = master.AddComponent<AISkillDriver>();
        component.customName = "StrafePickup";
        component.skillSlot = SkillSlot.None;
        component.minDistance = 20f;
        component.maxDistance = 30f;
        component.moveTargetType = AISkillDriver.TargetType.Custom;
        component.aimType = AISkillDriver.AimType.AtMoveTarget;
        component.movementType = AISkillDriver.MovementType.StrafeMovetarget;

        component = master.AddComponent<AISkillDriver>();
        component.customName = "BackUpFromPickup";
        component.skillSlot = SkillSlot.None;
        component.maxDistance = 20f;
        component.moveTargetType = AISkillDriver.TargetType.Custom;
        component.aimType = AISkillDriver.AimType.AtMoveTarget;
        component.movementType = AISkillDriver.MovementType.FleeMoveTarget;

        // The skill drivers must be in a specific order and I do not know of a better way to
        // programmatically reorder them, so I remove and readd the ones I want at the end.
        foreach (var skillDriver in originalSkillDrivers)
        {
            var copy = master.AddComponent<AISkillDriver>();
            foreach (var field in skillDriver.GetType().GetFields())
            {
                field.SetValue(copy, field.GetValue(skillDriver));
            }
            Component.Destroy(skillDriver);
        }
    }

    private void PatchPlayer()
    {
        var playerMaster = MasterCatalog.GetMasterPrefab(MasterCatalog.FindMasterIndex("PlayerMaster"));
        if (playerMaster == null)
        {
            Logger.LogError("PlayerMaster not found");
            return;
        }
        playerMaster.AddComponent<EquipmentDroneRecyclerController>().enabled = false;
    }
}