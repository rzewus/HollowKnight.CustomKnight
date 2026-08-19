using CustomKnight.NewUI;
using Satchel.BetterMenus;

namespace CustomKnight
{
    internal class AdvancedMenu
    {
        private static readonly float[] FontScales = { 1.0f, 0.95f, 0.90f, 0.85f, 0.80f };
        private static readonly string[] FontScaleLabels = { "100%", "95%", "90%", "85%", "80%" };

        private static Menu MenuRef;
        private static MenuScreen MenuScreenRef;
        internal static MenuScreen GetMenu(MenuScreen lastMenu)
        {
            if (MenuScreenRef == null)
            {
                if (MenuRef == null)
                {
                    MenuRef = PrepareMenu();
                }
                MenuScreenRef = MenuRef.GetMenuScreen(lastMenu);
            }
            else
            {
                MenuRef.returnScreen = lastMenu;
            }
            return MenuScreenRef;
        }

        private static Menu PrepareMenu()
        {

            var menu = new Menu("CK Advanced Options", new Element[]{
                new HorizontalOption(
                    "Swapper", "Apply skin to bosses, enemies & areas",
                    new string[] { "Disabled", "Enabled" },
                    (setting) => { CustomKnight.toggleSwap(setting == 1); },
                    () => CustomKnight.swapManager.enabled ? 1 : 0,
                    Id:"SwapperEnabled"),
                Blueprints.HorizontalBoolOption(
                    "Preload GameObjects for Events","(restart required)",
                    (v) => {
                        CustomKnight.GlobalSettings.Preloads = v;
                    },
                    () => CustomKnight.GlobalSettings.Preloads),
                Blueprints.HorizontalBoolOption(
                    "Enable Save Huds","(restart required)",
                    (v) => {
                        CustomKnight.GlobalSettings.EnableSaveHuds = v;
                    },
                    () => CustomKnight.GlobalSettings.EnableSaveHuds),
                Blueprints.HorizontalBoolOption(
                    "Enable Pause Menu UI","(restart required)",
                    (v) => {
                        CustomKnight.GlobalSettings.EnablePauseMenu = v;
                        if (CustomKnight.GlobalSettings.EnablePauseMenu)
                        {
                            UIController.EnableMenu();
                        } else
                        {
                            UIController.DisableMenu();
                        }
                    },
                    () => CustomKnight.GlobalSettings.EnablePauseMenu),
                new HorizontalOption(
                    "In-Game Font Size",
                    "Scale in-game text only (inventory, dialogues). Does not affect mod menus.",
                    FontScaleLabels,
                    (setting) =>
                    {
                        CustomKnight.GlobalSettings.InGameFontScale = FontScales[setting];
                        FontScaleManager.RefreshAll();
                    },
                    () =>
                    {
                        var s = CustomKnight.GlobalSettings.InGameFontScale;
                        for (var i = 0; i < FontScales.Length; i++)
                        {
                            if (System.Math.Abs(FontScales[i] - s) < 0.001f)
                            {
                                return i;
                            }
                        }
                        return 0;
                    },
                    Id: "InGameFontScale"),
                new MenuRow(
                    new List<Element>{
                        new MenuButton("Make Default","Creates the default skin on next restart",(_)=>RegenerateDefaultSkin()),
                        new MenuButton("Fix Skins","Attempts to Fix Skin Structure",(_)=>FixSkins()),
                        new MenuButton("Reset Settings","Reset CustomKnight Settings",(_)=>ResetSettings()),
                    },
                    Id:"AdditonalButtonGroup"
                ){ XDelta = 425f},

            });
            return menu;
        }

        private static void Back()
        {
            Utils.GoToMenuScreen(MenuRef.returnScreen);
        }

        private static void RegenerateDefaultSkin()
        {
            CustomKnight.GlobalSettings.GenerateDefaultSkin = true;
            GlobalModSettings.ResetProfileSkins();
            Back();
        }

        private static void ResetSettings()
        {
            CustomKnight.GlobalSettings = new GlobalModSettings();
            if (CustomKnight.SaveSettings != null)
            {
                CustomKnight.SaveSettings = new SaveModSettings();
            }
            Back();
        }


        private static void FixSkins()
        {
            FixSkinStructure.FixSkins();
            BetterMenu.ReloadSkins();

            Back();
        }
    }
}
