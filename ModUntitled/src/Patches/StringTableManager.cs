#pragma warning disable 0626
#pragma warning disable 0649

// Purpose:
// Add 'SynergyTable' and 'UITable' to StringTableManager

using System;
using MonoMod;
using System.Collections.Generic;

namespace ModUntitled.Patches {
    [MonoModPatch("global::StringTableManager")]
    public static class StringTableManager {
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_introTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_backupItemsTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_backupCoreTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_synergyTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_backupSynergyTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_backupIntroTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_uiTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_backupUiTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_enemiesTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_itemsTable;
        [MonoModIgnore]
        private static Dictionary<string, global::StringTableManager.StringCollection> m_coreTable;
        [MonoModIgnore]
        private static string m_currentSubDirectory;
        [MonoModIgnore]
        private static string m_currentFile;

        [MonoModIgnore]
        private extern static Dictionary<string, global::StringTableManager.StringCollection> LoadUITable(string subDirectory);
        [MonoModIgnore]
        private extern static Dictionary<string, global::StringTableManager.StringCollection> LoadSynergyTable(string subDirectory);

        public static Dictionary<string, global::StringTableManager.StringCollection> UITable {
            get {
                if (m_uiTable == null) {
                    m_uiTable = LoadUITable(m_currentSubDirectory);
                }
                if (m_backupUiTable == null) {
                    m_backupUiTable = LoadUITable("english_items");
                }
                return m_uiTable;
            }
        }

        public static Dictionary<string, global::StringTableManager.StringCollection> SynergyTable {
            get {
                if (m_synergyTable == null) {
                    m_synergyTable = LoadSynergyTable(m_currentSubDirectory);
                }
                if (m_backupSynergyTable == null) {
                    m_backupSynergyTable = LoadSynergyTable("english_items");
                }
                return m_synergyTable;
            }
        }
    }
}
