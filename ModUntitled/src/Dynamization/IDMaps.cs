using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace ModUntitled {
    public partial class ModUntitled : MonoBehaviour {
        [Flags]
        public enum ItemTags {
            Unknown = 2 << 0,
            Item = 2 << 1,
            Consumable = 2 << 2,
            Synergy = 2 << 3,

            Unused = 2 << 4,
            Internal = 2 << 5
        }

        [Flags]
        public enum EnemyTags {
            Unknown = 2 << 0,
            Enemy = 2 << 1,
            Friendly = 2 << 2,

            Unused = 2 << 3,
            Internal = 2 << 4
        }

        [Flags]
        public enum CharacterTags {
            Char = 2 << 0,
            Unlock = 2 << 1,
            Unused = 2 << 2
        }

        public static IDPool<PickupObject, ItemTags> Items;
        public static IDPool<AIActor, EnemyTags> Enemies;
        public static IDPool<PlayerController, CharacterTags> Characters;

        private IDPool<T, TagType> _ReadIDMapBraveResources<T, TagType>(StreamReader stream) where T : UnityEngine.Component where TagType : struct, IConvertible {
            return _ReadIDMap<T, TagType>((line_id, prefab_name) => {
                prefab_name = prefab_name.Replace("%%%", " ");

                var prefab = BraveResources.Load<GameObject>(prefab_name);
                if (prefab == null) throw new Exception($"Failed parsing ID map file: failed to load asset '{prefab_name}'");
                var component = prefab.GetComponent<T>();
                if (component == null) throw new Exception($"Failed parsing ID map file: successfully loaded asset '{prefab_name}', but it doesn't have a '{typeof(T).Name}' type component");
                return component;
            }, stream);
        }

        private IDPool<T, TagType> _ReadIDMapEnemyDB<T, TagType>(StreamReader stream) where T : UnityEngine.Component where TagType : struct, IConvertible {
            return _ReadIDMap<T, TagType>((line_id, prefab_name) => {
                prefab_name = prefab_name.Replace("%%%", " ");

                var prefab = EnemyDatabase.AssetBundle.LoadAsset<GameObject>(prefab_name);
                if (prefab == null) throw new Exception($"Failed parsing ID map file: failed to load asset '{prefab_name}'");
                var component = prefab.GetComponent<T>();
                if (component == null) throw new Exception($"Failed parsing ID map file: successfully loaded asset '{prefab_name}', but it doesn't have a '{typeof(T).Name}' type component");
                return component;
            }, stream);
        }

        private IDPool<T, TagType> _ReadIDMapList<T, TagType>(List<T> list, StreamReader stream) where T : UnityEngine.Object where TagType : struct, IConvertible {
            return _ReadIDMap<T, TagType>(
                obtain_func: (line_id, id_str) => {
                    int id;
                    if (!int.TryParse(id_str, out id)) throw new Exception($"Failed parsing ID map file: ID column at line {line_id} was not an integer");
                    return list[id];
                },
                stream: stream
            );
        }

        private IDPool<T, TagType> _ReadIDMap<T, TagType>(Func<int, string, T> obtain_func, StreamReader stream) where T : UnityEngine.Object where TagType : struct, IConvertible {
            var pool = new IDPool<T, TagType>();

            var line_id = 0;

            while (!stream.EndOfStream) {
                line_id += 1;
                var line = stream.ReadLine().Trim();
                if (line.StartsWithInvariant("#")) continue;
                if (line.Length == 0) continue;

                var split = line.Split(' ');
                if (split.Length < 3) {
                    throw new Exception($"Failed parsing ID map file: not enough columns at line {line_id} (need at least 2, ID and the name)");
                }
                var tags_split = split[0].Split(',');

                try {
                    var name_id = $"gungeon:{split[2]}";
                    pool[name_id] = obtain_func.Invoke(line_id, split[1]);

                    for (int i = 0; i < tags_split.Length; i++) {
                        var tag = (TagType)Enum.Parse(typeof(TagType), tags_split[i], true);
                        pool.AddTag(name_id, tag);
                    }
                } catch (Exception e) {
                    throw new Exception($"Failed loading ID map file: Error while adding entry to ID pool ({e.Message})");
                }
            }

            pool.LockNamespace("gungeon");
            return pool;
        }

        private StreamReader GetIDMapStream(string id) {
            var asm = Assembly.GetExecutingAssembly();
            var res_name = $"idmap:{id}";

            return new StreamReader(asm.GetManifestResourceStream(res_name));
        }

        private void SetupIDPools() {
            Logger.Debug("SetupIDPools()");

            Logger.Info("Loading item ID map");
            using (var stream = GetIDMapStream("items")) {
                Items = _ReadIDMapList<PickupObject, ItemTags>(PickupObjectDatabase.Instance.Objects, stream);
            }

            Logger.Info("Loading enemy ID map");
            using (var stream = GetIDMapStream("enemies")) {
                Enemies = _ReadIDMapEnemyDB<AIActor, EnemyTags>(stream);
            }

            Logger.Info("Loading character ID map");
            using (var stream = GetIDMapStream("characters")) {
                Characters = _ReadIDMapBraveResources<PlayerController, CharacterTags>(stream);
            }
        }
    }
}