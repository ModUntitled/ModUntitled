using System;
using ModTheGungeon;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace ModUntitled {
    public class ModUntitled : MonoBehaviour {
        public static Logger Logger = new Logger("ModUntitled");
        public static GameObject GameObject;
        public static ModUntitled Instance;

        public void GamePreload() {
            Logger.Info("Game Preload");
        }

        public void GameLoad() {
            Logger.Info("Game Load");
        }
    }
}