using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModUntitled {
    public partial class ModUntitled {

        public void SetupItems() {
            Logger.Debug("SetupItems()");

            ItemBuilder.Initialize();
            ChestReroller.Init();
            ScrollOfApproxKnowledge.Init();
        }
    }
}
