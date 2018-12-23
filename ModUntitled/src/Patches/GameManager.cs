#pragma warning disable 0626
#pragma warning disable 0649

///////////////////////////////
//// CONTINUATION OF ENTRY ////
///////////////////////////////

using System;
using ModUntitled;
using UnityEngine;
using System.Reflection;
using MonoMod;

namespace ModUntitled.Patches {
    [MonoModPatch("global::GameManager")]
    internal class GameManager : global::GameManager {
        protected extern void orig_Awake();
        private new void Awake() {
            ModUntitled.Logger.Info("GameManager is alive");
            ModUntitled.Instance.GameLoad();

            orig_Awake();
        }
    }
}