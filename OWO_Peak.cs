using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace OWO_PEAK
{
    [BepInPlugin("org.bepinex.plugins.OWO_PEAK", "OWO_PEAK", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109
        internal static new ManualLogSource Log;
#pragma warning restore CS0109

        public static OWOSkin owoSkin;
        private Harmony harmony;

        private void Awake()
        {
            Log = Logger;
            Logger.LogMessage("OWO_PEAK plugin is loaded!");

            owoSkin = new OWOSkin();

            harmony = new Harmony("owo.patch.peak");
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }

        [HarmonyPatch(typeof(Character))]
        internal static class Character_Patch
        {
            [HarmonyPatch(typeof(Character), "OnLand", MethodType.Normal)]
            [HarmonyPostfix]
            private static void Land(Character __instance, float sinceGrounded)
            {
                if (sinceGrounded <= 0.5)
                    return;

                if (__instance.IsLocal)
                {
                    owoSkin.LOG("YOU HAVE LANDED!");
                    owoSkin.StopClimbing();
                    owoSkin.StopSlipping();

                    owoSkin.Feel("Landing", 3);
                }
            }

            [HarmonyPatch(nameof(Character.RPCA_Fall))]
            [HarmonyPostfix]
            private static void Fall(Character __instance, float seconds)
            {
                if (!__instance.IsLocal)
                    return;

                owoSkin.LOG("YOU FELL");
                owoSkin.Feel("Fall",3);
            }

            [HarmonyPatch(nameof(Character.RPCA_PassOut))]
            [HarmonyPostfix]
            private static void PassOut(Character __instance)
            {
                if (!__instance.IsLocal)
                    return;

                owoSkin.LOG("PASSED OUT");
                owoSkin.StartHeartBeat();
            }

            [HarmonyPatch(nameof(Character.RPCA_Die))]
            [HarmonyPostfix]
            private static void Die(Character __instance, Vector3 itemSpawnPoint)
            {
                if (!__instance.IsLocal)
                    return;

                owoSkin.LOG("YOU ARE DEAD");
                owoSkin.StopHeartBeat();
                owoSkin.Feel("Death",4);
            }
            [HarmonyPatch(typeof(Character), "RPCA_Revive", MethodType.Normal)]
            [HarmonyPostfix]
            private static void Revive(Character __instance, bool applyStatus)
            {
                if (!__instance.IsLocal)
                    return;

                owoSkin.LOG("REVIVED!");
                owoSkin.StopHeartBeat();
                owoSkin.Feel("Revive",3);
            }

            [HarmonyPatch(typeof(Character), "UnPassOutDone", MethodType.Normal)]
            [HarmonyPostfix]
            private static void UnPassOut(Character __instance)
            {
                if (!__instance.IsLocal)
                    return;

                owoSkin.LOG("REVIVED!");
                owoSkin.StopHeartBeat();
                owoSkin.Feel("Revive");
            }


            [HarmonyPatch(typeof(Character), "OutOfStamina", MethodType.Normal)]
            [HarmonyPrefix]
            private static bool OutOfStamina(Character __instance)
            {
                if ((double)__instance.data.currentStamina < 0.004999999888241291 && (double)__instance.data.extraStamina < 1.0 / 1000.0)
                {
                    if (__instance.data.isClimbingAnything)
                    {
                        owoSkin.StartSlipping();
                    }
                    return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterMovement))]
        internal static class CharacterMovement_Patch
        {
            [HarmonyPatch(nameof(CharacterMovement.JumpRpc))]
            [HarmonyPostfix]
            private static void RPC_Jump(CharacterMovement __instance, bool isPalJump)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (character != null && character.IsLocal)
                {
                    owoSkin.LOG("YOU HAVE JUMPED!");
                    owoSkin.Feel("Jump", 2);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterAfflictions))]
        internal static class CharacterAfflictions_Patch
        {
            [HarmonyPatch(nameof(CharacterAfflictions.AddStatus))]
            [HarmonyPostfix]
            private static void AddStatus(CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, float amount, bool fromRPC = false)
            {
                bool inAirport = Traverse.Create(__instance).Field("m_inAirport").GetValue<bool>();

                if (__instance.character.isBot || __instance.character.statusesLocked || (double)amount == 0.0 || inAirport)
                    return;

                if (!__instance.character.IsLocal)
                    return;

                if (statusType == CharacterAfflictions.STATUSTYPE.Injury)
                {
                    owoSkin.LOG("OUCH, FALL DAMAGE!");
                    owoSkin.Feel("Impact", 2);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterClimbing))]
        internal static class CharacterClimbing_Patch
        {
            [HarmonyPatch(typeof(CharacterClimbing), "StartClimbRpc", MethodType.Normal)]
            [HarmonyPostfix]
            private static void StartClimbing(CharacterClimbing __instance, Vector3 climbPos, Vector3 climbNormal)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (character.IsLocal && character.data.sinceGrounded <= 0.5)
                {
                    owoSkin.LOG("START CLIMBING!");
                    owoSkin.StartClimbing();
                }
            }

            [HarmonyPatch(nameof(CharacterClimbing.StopClimbingRpc))]
            [HarmonyPostfix]
            private static void RPC_StopClimbing(CharacterClimbing __instance, float setFall)
            {
                PhotonView view = Traverse.Create(__instance).Field("view").GetValue<PhotonView>();

                if (view.IsMine)
                {
                    owoSkin.LOG("STOP CLIMBING!");
                    owoSkin.StopClimbing();
                }
            }

            [HarmonyPatch(nameof(CharacterClimbing.RPCA_ClimbJump))]
            [HarmonyPostfix]
            private static void RPC_ClimbJump(CharacterClimbing __instance)
            {
                PhotonView view = Traverse.Create(__instance).Field("view").GetValue<PhotonView>();

                if (view.IsMine)
                {
                    owoSkin.LOG("CLIMB JUMP!!!");
                    owoSkin.Feel("Climb Jump", 2);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterItems))]
        internal static class CharacterItems_Patch
        {
            [HarmonyPatch(nameof(CharacterItems.OnPickupAccepted))]
            [HarmonyPostfix]
            private static void PickupItem(CharacterItems __instance)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (character.IsLocal)
                {
                    owoSkin.LOG("PICKUP ITEM!");
                    owoSkin.Feel("Pickup Item", 2);
                }
            }

            [HarmonyPatch(nameof(CharacterItems.DropItemRpc))]
            [HarmonyPostfix]
            private static void DropItemRPC(CharacterItems __instance, float throwCharge, byte slotID, Vector3 spawnPos, Vector3 velocity, Quaternion rotation, ItemInstanceData itemInstanceData)
            {
                if (!__instance.photonView.IsMine)
                    return;

                owoSkin.LOG("DROP ITEM!");
                owoSkin.Feel("Drop Item", 2);
            }
        }

        [HarmonyPatch(typeof(CharacterRopeHandling))]
        internal static class CharacterClimbingRope_Patch
        {
            [HarmonyPatch(nameof(CharacterRopeHandling.GrabRopeRpc))]
            [HarmonyPostfix]
            private static void StartClimbingRope(CharacterRopeHandling __instance, PhotonView ropeView, int segmentIndex)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();
                if (!character.IsLocal)
                    return;

                Rope componentInChildren = ropeView.GetComponentInChildren<Rope>();
                if ((UnityEngine.Object)componentInChildren == (UnityEngine.Object)null)
                    return;

                owoSkin.LOG("START ROPE CLIMBING!");
                owoSkin.StartClimbingRope();
            }

            [HarmonyPatch(typeof(CharacterRopeHandling), "StopRopeClimbingRpc", MethodType.Normal)]
            [HarmonyPostfix]
            private static void RPC_StopClimbingRope(CharacterRopeHandling __instance)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();
                if (!character.IsLocal)
                    return;

                owoSkin.LOG("STOP ROPE CLIMBING!");
                owoSkin.StopClimbingRope();
            }
        }

        [HarmonyPatch(typeof(CharacterVineClimbing))]
        internal static class CharacterVineClimbing_Patch
        {
            [HarmonyPatch(nameof(CharacterVineClimbing.GrabVineRpc))]
            [HarmonyPostfix]
            private static void StartClimbingVine(CharacterVineClimbing __instance, PhotonView ropeView, int segmentIndex)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();
                if (!character.IsLocal)
                    return;

                JungleVine component = ropeView.GetComponent<JungleVine>();
                if ((UnityEngine.Object)component == (UnityEngine.Object)null)
                    return;

                owoSkin.LOG("START VINE CLIMBING!");
                owoSkin.StartClimbingRope();
            }

            [HarmonyPatch(typeof(CharacterVineClimbing), "StopVineClimbingRpc", MethodType.Normal)]
            [HarmonyPostfix]
            private static void RPC_StopClimbingVine(CharacterVineClimbing __instance)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();
                if (!character.IsLocal)
                    return;

                owoSkin.LOG("STOP VINE CLIMBING!");
                owoSkin.StopClimbingRope();
            }
        }

        [HarmonyPatch(typeof(CharacterCarrying))]
        internal static class CharacterCarrying_Patch
        {
            [HarmonyPatch(nameof(CharacterCarrying.RPCA_StartCarry))]
            [HarmonyPostfix]
            private static void StartCarryingCharacter(CharacterCarrying __instance, PhotonView targetView)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (!character.photonView.IsMine)
                    return;

                owoSkin.LOG($"START CARRYING CHARACTER!");
                owoSkin.StartCarryingCharacter();
            }

            [HarmonyPatch(nameof(CharacterCarrying.RPCA_Drop))]
            [HarmonyPostfix]
            private static void DropCharacter(CharacterCarrying __instance, PhotonView targetView)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (!character.photonView.IsMine)
                    return;

                owoSkin.LOG("DROP CHARACTER!");
                owoSkin.StopCarryingCharacter();
            }
        }


        [HarmonyPatch(typeof(GUIManager))]
        internal static class GUIManager_Patch
        {
            [HarmonyPatch(nameof(GUIManager.Grasp))]
            [HarmonyPostfix]
            private static void Grab_Character(GUIManager __instance)
            {
                Character character = Traverse.Create(__instance).Field("character").GetValue<Character>();

                if (!character.photonView.IsMine)
                    return;

                owoSkin.GrabCharacter();

            }
        }

        [HarmonyPatch(typeof(Item))]
        internal static class Item_Patch
        {
            [HarmonyPatch(nameof(Item.Consume))]
            [HarmonyPostfix]
            private static void ConsumeItem(Item __instance, int consumerID)
            {
                PhotonView view = Traverse.Create(__instance).Field("view").GetValue<PhotonView>();

                if (consumerID != -1 && __instance.holderCharacter.IsLocal)
                {
                    owoSkin.LOG($"CONSUMED ITEM!");
                    owoSkin.Feel("Eating", 2);
                }
            }
        }

        [HarmonyPatch(typeof(Rope))]
        internal static class Rope_Patch
        {
            [HarmonyPatch(nameof(Rope.AttachToSpool_Rpc))]
            [HarmonyPostfix]
            private static void AttachToRopeSpool(Rope __instance, PhotonView viewSpool)
            {
                if (!viewSpool.IsMine)
                    return;

                owoSkin.LOG("ANCHORED TO SPOOL!");
                owoSkin.Feel("Rope Anchor",2);
            }
        }

        [HarmonyPatch(typeof(BugleSFX))]
        internal static class Bugle_Patch
        {
            [HarmonyPatch(typeof(BugleSFX), "RPC_StartToot", MethodType.Normal)]
            [HarmonyPostfix]
            private static void StartToot(BugleSFX __instance, int clip)
            {
                PhotonView view = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();

                if (!view.IsMine)
                    return;

                owoSkin.LOG("START BOOOGLING!");
                owoSkin.StartTooting();
            }

            [HarmonyPatch(typeof(BugleSFX), "RPC_EndToot", MethodType.Normal)]
            [HarmonyPostfix]
            private static void StopToot(BugleSFX __instance)
            {
                PhotonView view = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();

                if (!view.IsMine)
                    return;

                owoSkin.LOG("END BUGLE!");
                owoSkin.StopTooting();
            }
        }
    }
}

