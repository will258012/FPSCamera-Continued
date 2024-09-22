using AlgernonCommons.Patching;
using FPSCamera.Cam.Controller;
using FPSCamera.Settings;
using FPSCamera.UI;
using FPSCamera.Utils;
using ICities;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCamera
{
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor };

        /// <summary>
        /// Checks for any mod conflicts.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>A list of conflicting mod names (null or empty if none).</returns>
        protected override List<string> CheckModConflicts() => ModSupport.CheckModConflicts();
        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            Object.Destroy(gameObject);
            base.OnLevelUnloading();
        }
        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);
            ModSupport.Initialize();
            gameObject = new GameObject();
            gameObject.AddComponent<FPSCamController>();
            gameObject.AddComponent<CamInfoPanel>();
            gameObject.AddComponent<MainPanel>();
            if (ToolsModifierControl.isGame)
                gameObject.AddComponent<FollowButtons>();
        }

        private GameObject gameObject = new GameObject();
    }
}
