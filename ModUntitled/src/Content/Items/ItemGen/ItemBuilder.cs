﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections;

namespace ModUntitled
{
    public static class ItemBuilder
    {
        private static tk2dSpriteCollectionData _AmmonomiconCollection;
        private static tk2dSpriteCollectionData _ItemCollection;
        private static tk2dSpriteCollectionData _WeaponCollection;
        private static tk2dSpriteCollectionData _WeaponCollection02;

        internal static void Initialize() {
            _AmmonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
            var sprites = UnityEngine.Object.FindObjectsOfType<tk2dBaseSprite>();
            for (int i = 0; i < sprites.Length; i++) {
                var sprite = sprites[i];

                if (sprite?.Collection == null) continue;

                if (sprite.Collection.spriteCollectionName == "ItemCollection") {
                    _ItemCollection = sprite.Collection;
                }
                if (sprite.Collection.spriteCollectionName == "WeaponCollection") {
                    _WeaponCollection = sprite.Collection;
                }
                if (sprite.Collection.spriteCollectionName == "WeaponCollection02") {
                    _WeaponCollection02 = sprite.Collection;
                }
            }
        }

        public enum CooldownType
        {
            Timed, Damage, PerRoom, None
        }

        /// <summary>
        /// Creates an object with a sprite component and adds that sprite to the 
        /// ammonomicon for later use.
        /// </summary>
        public static GameObject CreateSpriteObject(string name, string resourcePath)
        {
            GameObject spriteObject = SpriteBuilder.SpriteFromResource(_ItemCollection, resourcePath);
            spriteObject.name = name;
            return spriteObject;
        }

        /// <summary>
        /// Adds a sprite definition to the Ammonomicon sprite collection
        /// </summary>
        /// <returns>The spriteID of the defintion in the ammonomicon collection</returns>
        public static int AddSpriteToAmmonomicon(tk2dSpriteDefinition spriteDefinition) {
            return SpriteBuilder.AddSpriteToCollection(spriteDefinition, _AmmonomiconCollection);
        }

        public static GameObject ItemSpriteFromResource(string spriteName) {
            string extension = !spriteName.EndsWith(".png") ? ".png" : "";
            string resourcePath = spriteName + extension;

            var texture = ResourceExtractor.GetTextureFromResource(resourcePath);
            if (texture == null) return null;

            return SpriteBuilder.SpriteFromTexture(_ItemCollection, texture, resourcePath);
        }

        /// <summary>
        /// Finishes the item setup, adds it to the item databases, adds an encounter trackable 
        /// blah, blah, blah
        /// </summary>
        public static void SetupItem(PickupObject item, string shortDesc, string longDesc, string idPool = "customItems")
        {
            if (item.encounterTrackable == null) item.encounterTrackable = item.gameObject.AddComponent<EncounterTrackable>();
            if (item.encounterTrackable.journalData == null) item.encounterTrackable.journalData = new JournalEntry();

            item.encounterTrackable.EncounterGuid = item.name;

            item.encounterTrackable.prerequisites = new DungeonPrerequisite[0];
            item.encounterTrackable.journalData.SuppressKnownState = true;

            string keyName = "#" + item.name.Replace(" ", "").ToUpperInvariant();
            var disp_name = item.encounterTrackable.journalData.PrimaryDisplayName = keyName + "_ENCNAME";
            var short_desc = item.encounterTrackable.journalData.NotificationPanelDescription = keyName + "_SHORTDESC";
            var long_desc = item.encounterTrackable.journalData.AmmonomiconFullEntry = keyName + "_LONGDESC";
            item.encounterTrackable.journalData.AmmonomiconSprite = item.name.Replace(' ', '_') + "_idle_001";

            I18N.AddItemString(disp_name, item.name);
            I18N.AddItemString(short_desc, shortDesc);
            I18N.AddItemString(long_desc, longDesc);

            AddSpriteToAmmonomicon(item.sprite.GetCurrentSpriteDef());
            item.encounterTrackable.journalData.AmmonomiconSprite = item.sprite.GetCurrentSpriteDef().name;

            if(item is PlayerItem)
            (item as PlayerItem).consumable = false;

            ModUntitled.Items.Add($"{idPool}:{item.name.ToLower().Replace(" ", "_")}", item);
        }

        /// <summary>
        /// Sets the cooldown type and length of a PlayerItem, and resets all other cooldown types
        /// </summary>
        public static void SetCooldownType(PlayerItem item, CooldownType cooldownType, float value)
        {
            item.damageCooldown = -1;
            item.roomCooldown = -1;
            item.timeCooldown = -1;

            switch (cooldownType)
            {
                case CooldownType.Timed:
                    item.timeCooldown = value;
                    break;
                case CooldownType.Damage:
                    item.damageCooldown = value;
                    break;
                case CooldownType.PerRoom:
                    item.roomCooldown = (int)value;
                    break;
            }
        }

        /// <summary>
        /// Adds a passive player stat modifier to a PlayerItem or PassiveItem
        /// </summary>
        public static void AddPassiveStatModifier(PickupObject po, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier();
            modifier.amount = amount;
            modifier.statToBoost = statType;
            modifier.modifyType = method;

            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem or PassiveItem");
            }
        }

        public static IEnumerator HandleDuration(PlayerItem item, float duration, PlayerController user, Action<PlayerController> OnFinish)
        {
            if (item.IsCurrentlyActive)
            {
                yield break;
            }

            SetPrivateType<PlayerItem>(item, "m_isCurrentlyActive", true);
            SetPrivateType<PlayerItem>(item, "m_activeElapsed", 0f);
            SetPrivateType<PlayerItem>(item, "m_activeDuration", duration);
            item.OnActivationStatusChanged?.Invoke(item);

            float elapsed = GetPrivateType<PlayerItem, float>(item, "m_activeElapsed");
            float dur = GetPrivateType<PlayerItem, float>(item, "m_activeDuration");

            while (GetPrivateType<PlayerItem, float>(item, "m_activeElapsed") < GetPrivateType<PlayerItem, float>(item, "m_activeDuration") && item.IsCurrentlyActive)
            {
                yield return null;
            }
            SetPrivateType<PlayerItem>(item, "m_isCurrentlyActive", false);
            item.OnActivationStatusChanged?.Invoke(item);

            OnFinish?.Invoke(user);
            yield break;
        }

        private static void SetPrivateType<T>(T obj, string field, bool value)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(obj, value);
        }

        private static void SetPrivateType<T>(T obj, string field, float value)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(obj, value);
        }

        private static T2 GetPrivateType<T, T2>(T obj, string field)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T2)f.GetValue(obj);
        }
    }
}
