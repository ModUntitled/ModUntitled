using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ChestType = Chest.GeneralChestType;

namespace ModUntitled
{
    class ScrollOfApproxKnowledge : PassiveItem
    {
        static string gunTextSpritePath = "items.ScrollOfApproximateKnowledge.gun_text";
        static string itemTextSpritePath = "items.ScrollOfApproximateKnowledge.item_text";
        static List<Tuple<Chest, int>> foundChests = new List<Tuple<Chest, int>>();


        public static void Init()
        {
            string itemName = "Scroll of Approximate Knowledge"; //The name of the item
            string resourceName = "items.ScrollOfApproximateKnowledge.approx_scroll"; //Refers to an embedded png in the project. Make sure to embed your resources!

            //Generate a new GameObject with a sprite component
            GameObject spriteObj = ItemBuilder.CreateSpriteObject(itemName, resourceName);

            //Add a PassiveItem component to the object
            PassiveItem item = spriteObj.AddComponent<ScrollOfApproxKnowledge>();

            //Ammonomicon entry variables
            string shortDesc = "It's definitely something...";
            string longDesc = "Vaguely describes the contents of chests.\n\n" +
                "It is said that this magically embued toilet paper roll was an Apprentice Gunjurer's last-ditch " +
                "attempt at passing their graduation exam. The scroll can " +
                "make a pretty good guess about anything in the Gungeon, but I wouldn't put too much faith in it.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "moduntitled");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            if (player.GetComponent<GuessDisplayBehaviour>() != null)
            {
                player.GetComponent<GuessDisplayBehaviour>().Destroy();
            }
            player.gameObject.AddComponent<GuessDisplayBehaviour>().parent = this;
        }

        private class GuessDisplayBehaviour : BraveBehaviour
        {
            public ScrollOfApproxKnowledge parent;
            private tk2dSprite textSprite;
            tk2dSpriteCollectionData collection;
            private float size;
            private float maxSize;
            private int gunID;
            private int itemID;
            Chest lastChest;
            Vector2 baseScale;
            Vector2 basePos;

            void Start()
            {
                BuildCollection();
            }

            void FixedUpdate()
            {
                PlayerController player = this.GetComponent<PlayerController>();
                if (player == null || !player.passiveItems.Contains(parent))
                {
                    Destroy();
                    return;
                }
                IPlayerInteractable nearestInteractable = player.CurrentRoom.GetNearestInteractable(player.CenterPosition, 1f, player);

                if (!(nearestInteractable is Chest))
                {
                    HandleSize(Vector2.zero);
                    return;
                }

                Chest chest = nearestInteractable as Chest;
                if (chest != lastChest)
                {
                    Setup(chest);
                }
                HandleSize(baseScale);
            }

            void HandleSize(Vector2 targetScale)
            {
                if (Vector2.Distance(textSprite.scale, targetScale) < .05f)
                {
                    textSprite.scale = targetScale;
                    return;
                }
                textSprite.scale = Vector2.Lerp(textSprite.scale, targetScale, Time.deltaTime * 10f);

                textSprite.PlaceAtPositionByAnchor(basePos, tk2dBaseSprite.Anchor.LowerCenter);
            }

            void Setup(Chest chest)
            {
                lastChest = chest;
                var type = chest.ChestType;

                int spriteID;
                int chestID = ChestHasBeenFound(chest);
                if (chestID >= 0)
                {
                    spriteID = foundChests[chestID].Second;
                }
                else
                {
                    spriteID = ChooseID(type);
                    foundChests.Add(new Tuple<Chest, int>(chest, spriteID));
                }

                if (collection == null || !collection.IsValidSpriteId(spriteID))
                {
                    BuildCollection();
                }
                textSprite.SetSprite(collection, spriteID);
                textSprite.scale = baseScale;
                basePos = chest.sprite.WorldTopCenter + new Vector2(0, .25f);
                textSprite.PlaceAtLocalPositionByAnchor(basePos, tk2dBaseSprite.Anchor.LowerCenter);

                textSprite.scale = Vector2.zero;
                textSprite.SortingOrder = 100;
                SpriteOutlineManager.AddOutlineToSprite<tk2dSprite>(textSprite.sprite, new Color(.8f, 0, .5f), 0.4f, 0f);
            }

            int ChooseID(ChestType type)
            {
                int id;
                if (type == ChestType.WEAPON)
                    id = gunID;
                else if (type == ChestType.ITEM)
                    id = itemID;
                else
                    id = UnityEngine.Random.value < .5f ? gunID : itemID;

                //Get it wrong sometimes
                if (UnityEngine.Random.value < .25f)
                    id = UnityEngine.Random.value < .5f ? gunID : itemID;

                return id;
            }

            int ChestHasBeenFound(Chest chest)
            {
                for (int i = 0; i < foundChests.Count; i++)
                {
                    var tuple = foundChests[i];
                    if (tuple.First == chest)
                        return i;
                }
                return -1;
            }

            public void BuildCollection()
            {
                if (collection != null)

                    Destroy(collection);

                if (textSprite != null)
                    Destroy(textSprite);

                String name = "SCROLL_COLLECTION";
                collection = new tk2dSpriteCollectionData();
                collection.assetName = name;
                collection.allowMultipleAtlases = false;
                collection.buildKey = 0x0ade;
                collection.dataGuid = name + "_DATAGUID";
                collection.spriteCollectionGUID = name + "_GUID";
                collection.spriteCollectionName = name;
                collection.spriteDefinitions = new tk2dSpriteDefinition[0];
                DontDestroyOnLoad(collection);

                gunID = SpriteBuilder.AddSpriteToCollection(gunTextSpritePath, collection);
                itemID = SpriteBuilder.AddSpriteToCollection(itemTextSpritePath, collection);

                textSprite = SpriteBuilder.SpriteFromResource(collection, ScrollOfApproxKnowledge.gunTextSpritePath).GetComponent<tk2dSprite>();

                baseScale = textSprite.scale;
                textSprite.scale = Vector2.zero;
            }

            public void Destroy()
            {
                Destroy(textSprite);
                Destroy(this);
            }
        }
    }
}
