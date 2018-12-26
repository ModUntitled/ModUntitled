using System;
using System.Collections.Generic;

namespace ModUntitled {
    public static class I18N {
        private static Dictionary<StringTableManager.GungeonSupportedLanguages, Dictionary<string, StringTableManager.StringCollection>> _StringCache;

        private static void _AddString(Dictionary<string, StringTableManager.StringCollection> dict, string key, string value) {
            if (!key.StartsWithInvariant("#")) key = $"#{key}";
            var coll = dict[key] = new StringTableManager.SimpleStringCollection();
            coll.AddString(value, 1f);
        }

        public static void AddItemString(string key, string value) {
            _AddString(StringTableManager.ItemTable, key, value);
        }

        public static void AddEnemyString(string key, string value) {
            _AddString(StringTableManager.EnemyTable, key, value);
        }

        public static void AddCoreString(string key, string value) {
            _AddString(StringTableManager.CoreTable, key, value);
        }

        public static void AddIntroString(string key, string value) {
            _AddString(StringTableManager.IntroTable, key, value);
        }

        public static void AddUIString(string key, string value) {
            _AddString(Patches.StringTableManager.UITable, key, value);
        }

        public static void AddSynergyString(string key, string value) {
            _AddString(Patches.StringTableManager.SynergyTable, key, value);
        }
    }
}
