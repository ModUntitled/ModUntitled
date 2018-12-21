#pragma warning disable 0626
#pragma warning disable 0649

// Purpose:
// 'Instance' field in MMFC

using System;
using ModTheGungeon;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using MonoMod;
using System.IO;
using System.Collections.Generic;

namespace ModUntitled.Patches {
    [MonoModPatch("global::MainMenuFoyerController")]
    public class MainMenuFoyerController : global::MainMenuFoyerController {
        public static MainMenuFoyerController Instance;

        protected extern void orig_Awake();
        private new void Awake() {
            Instance = this;

            orig_Awake();
        }
    }
}
