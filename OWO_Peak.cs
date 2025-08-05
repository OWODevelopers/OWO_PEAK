using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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


        //[HarmonyPatch(typeof(Attack), nameof(Attack.OnAttackTrigger))]
        //class OnWeaponAttack
        //{
        //    public static Dictionary<string, float> rangeBoss = new Dictionary<string, float>()
        //{
        //    {"boss_eikthyr",  20f},
        //    {"boss_gdking", 40f },
        //    {"boss_bonemass",  20f},
        //    {"boss_moder",  40f},
        //    {"boss_goblinking",  60f},
        //};
        //    public static void Postfix(Attack __instance, Humanoid ___m_character, ItemDrop.ItemData ___m_weapon)
        //    {
        //        if (!owoSkin.CanFeel()) return;

        //        if (___m_character.IsBoss()) goto Boss;

        //        if (___m_character == Player.m_localPlayer) goto Player;

        //        return;


        //    Boss:
        //        float range = (rangeBoss.ContainsKey(___m_character.m_bossEvent)) ? rangeBoss[___m_character.m_bossEvent] : 20f;
        //        bool closeTo = Player.IsPlayerInRange(___m_character.transform.position, range, Player.m_localPlayer.GetPlayerID());

        //        if (!closeTo) return;

        //        switch (___m_character.m_bossEvent)
        //        {
        //            case "boss_eikthyr":
        //                if (__instance.m_attackAnimation == "attack2")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "attack_stomp")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                break;
        //            case "boss_gdking":
        //                if (__instance.m_attackAnimation == "spawn")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "stomp")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "shoot")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                break;
        //            case "boss_bonemass":
        //                if (__instance.m_attackAnimation == "aoe")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                break;
        //            case "boss_moder":
        //                if (__instance.m_attackAnimation == "attack_iceball")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "attack_breath")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                break;
        //            case "boss_goblinking":
        //                if (__instance.m_attackAnimation == "beam")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "nova")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                if (__instance.m_attackAnimation == "cast1")
        //                {
        //                    owoSkin.Feel("Earthquake", 2);
        //                }
        //                break;

        //        }
        //        return;


        //    Player:
        //        owoSkin.LOG($"HUMANOID {___m_weapon.m_shared.m_itemType} -- {___m_weapon.m_shared.m_animationState} -- {___m_weapon.m_shared.m_name}");
        //        switch (___m_weapon.m_shared.m_itemType)
        //        {
        //            case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
        //            case ItemDrop.ItemData.ItemType.Bow:
        //                owoSkin.FeelWithMuscles("Attack","Both Arms", 3);
        //                break;
        //            default:
        //                owoSkin.FeelWithMuscles("Attack","Right Arm", 3);
        //                break;
        //        }
        //        return;
        //    }
        //}

        //[HarmonyPatch(typeof(Character), "SetHealth")]
        //class OnPlayerSetHealth
        //{
        //    public static void Postfix(Character __instance)
        //    {
        //        if (__instance != Player.m_localPlayer || !owoSkin.CanFeel()) return;
        //        int hp = Convert.ToInt32(__instance.GetHealth() * 100 / __instance.GetMaxHealth());

        //        if (hp <= 0)
        //        {
        //            owoSkin.StopAllHapticFeedback();
        //            owoSkin.playerEnabled = false;
        //            owoSkin.Feel("Death", 4);
        //        }
        //        else if (hp < 20 && hp > 0)
        //        {
        //            owoSkin.StartHeartBeat();
        //        }
        //        else
        //        {
        //            owoSkin.StopHeartBeat();
        //        }
        //    }
        //}



        //[HarmonyPatch(typeof(Ragdoll), "DestroyNow")]
        //class OnCorpseExplosion
        //{
        //    public static void Postfix(Ragdoll __instance)
        //    {
        //        if (!owoSkin.CanFeel() || !Player.IsPlayerInRange(__instance.transform.position, 20f, Player.m_localPlayer.GetPlayerID())) return;
        //        foreach (EffectList.EffectData obj in __instance.m_removeEffect.m_effectPrefabs)
        //        {
        //            switch (obj.m_prefab.name)
        //            {
        //                case "vfx_corpse_destruction_medium":
        //                    goto SendSensation;
        //                case "vfx_corpse_destruction_small":
        //                    if (!Player.IsPlayerInRange(__instance.transform.position, 7f, Player.m_localPlayer.GetPlayerID())) return;
        //                    goto SendSensation;
        //            }
        //        }

        //    SendSensation:
        //        owoSkin.Feel("Explosion", 2);
        //    }
        //}

        //[HarmonyPatch(typeof(Character), "OnCollisionStay")]
        //class OnPlayerLand
        //{
        //    public static void Postfix(Player __instance, bool ___m_groundContact)
        //    {
        //        if (!owoSkin.CanFeel() || __instance != Player.m_localPlayer) return;
        //        if (owoSkin.isJumping && ___m_groundContact)
        //        {
        //            owoSkin.isJumping = false;
        //            owoSkin.Feel("Landing");
        //        }
        //    }
        //}
    }
}
