using System;
using ModTheGungeon;
using UnityEngine;
using Logger = ModTheGungeon.Logger;
using SGUI;

namespace ModUntitled {
    public class ModUntitled : MonoBehaviour {
        public const string VERSION = "0.1 DEV";
        private const float _FPS_INTERVAL = 1f;

        public static Logger Logger = new Logger("ModUntitled");
        public static GameObject GameObject;
        public static ModUntitled Instance;
        public static SGUIRoot GUIRoot;
        public static Font GungeonFont;

        public static DebugConsole.Console DebugConsole;

        private SGroup _DebugGroup = null;
        private SLabel _FPSLabel = null;
        private SLabel _VersionLabel = null;

        private float _FPSIntervalLeft = 0f;

        public void GamePreload() {
            Logger.Info("Game Preload");
        }

        public void GameLoad() {
            Logger.Info("Game Load");

            SetupSGUI();
            SetupDebugText();
            SetupDebugConsole();
        }

        public void SetupSGUI() {
            Logger.Debug("SetupSGUI()");

            GUIRoot = SGUIRoot.Setup();
            SGUIIMBackend.GetFont = (SGUIIMBackend backend) => {
                if (Patches.MainMenuFoyerController.Instance?.VersionLabel == null) return null;
                return GungeonFont ?? (GungeonFont = FontConverter.DFFontToUnityFont((dfFont)Patches.MainMenuFoyerController.Instance.VersionLabel.Font, 2));
            };
        }

        public void SetupDebugText() {
            Logger.Debug("SetupDebugText()");

            _DebugGroup = new SGroup {
                AutoLayout = (self) => self.AutoLayoutVertical,
                Visible = true,
                OnUpdateStyle = (elem) => {
                    elem.Fill(0);
                },
                Background = new Color(0.0f, 0.0f, 0.0f, 0.0f)
            };

            _DebugGroup.Children.Add(_VersionLabel = new SLabel($"ModUntitled v{VERSION}"));
            _DebugGroup.Children.Add(_FPSLabel = new SLabel("?? FPS"));
        }

        public void SetupDebugConsole() {
            Logger.Debug("SetupDebugConsole()");

            DebugConsole = new DebugConsole.Console();
        }

        ////

        public void Update() {
            _UpdateFPSLabel();
            _UpdateDebugConsole();
        }

        private void _UpdateFPSLabel() {
            if (_FPSLabel == null) return;

            _FPSIntervalLeft -= Time.unscaledDeltaTime;

            if (_FPSIntervalLeft <= 0) {
                _FPSIntervalLeft = _FPS_INTERVAL;

                var fps = 1.0f / Time.unscaledDeltaTime;
                var fpstext = $"{Math.Floor(fps)} FPS";
                _FPSLabel.Text = fpstext;
            }
        }

        private void _UpdateDebugConsole() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                if (DebugConsole.IsOpen) DebugConsole.Hide();
                else DebugConsole.Show();
            }
        }
    }
}