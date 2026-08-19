using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomKnight
{
    internal static class FontScaleManager
    {
        private static readonly HashSet<SetTextMeshProGameText> tracked = new();
        private static readonly Dictionary<int, float> baseFontSizes = new();
        private static bool hooked;

        private static FieldInfo tmproField;
        private static PropertyInfo fontSizeProperty;
        private static MethodInfo forceMeshUpdateMethod;

        internal static void Hook()
        {
            if (hooked)
            {
                return;
            }
            hooked = true;
            CacheReflection();
            On.SetTextMeshProGameText.Awake += OnAwake;
            On.SetTextMeshProGameText.UpdateText += OnUpdateText;
            On.ChangeFontByLanguage.SetFont += OnChangeFontByLanguageSetFont;
            ModHooks.SetFontHook += OnSetFontHook;
        }

        internal static void Unhook()
        {
            if (!hooked)
            {
                return;
            }
            hooked = false;
            On.SetTextMeshProGameText.Awake -= OnAwake;
            On.SetTextMeshProGameText.UpdateText -= OnUpdateText;
            On.ChangeFontByLanguage.SetFont -= OnChangeFontByLanguageSetFont;
            ModHooks.SetFontHook -= OnSetFontHook;
            tracked.Clear();
            baseFontSizes.Clear();
        }

        internal static void RefreshAll()
        {
            foreach (var self in tracked.ToList())
            {
                if (self != null)
                {
                    self.UpdateText();
                }
            }
        }

        private static void CacheReflection()
        {
            tmproField = typeof(SetTextMeshProGameText).GetField("tmpro", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var tmpType = tmproField?.FieldType;
            if (tmpType != null)
            {
                fontSizeProperty = tmpType.GetProperty("fontSize", BindingFlags.Instance | BindingFlags.Public);
                forceMeshUpdateMethod = tmpType.GetMethod("ForceMeshUpdate", BindingFlags.Instance | BindingFlags.Public, null, System.Type.EmptyTypes, null);
            }
        }

        private static void OnAwake(On.SetTextMeshProGameText.orig_Awake orig, SetTextMeshProGameText self)
        {
            orig(self);
            if (self != null)
            {
                tracked.Add(self);
            }
        }

        private static void OnUpdateText(On.SetTextMeshProGameText.orig_UpdateText orig, SetTextMeshProGameText self)
        {
            orig(self);
            CaptureBaseAndApply(self);
        }

        private static void OnChangeFontByLanguageSetFont(On.ChangeFontByLanguage.orig_SetFont orig, ChangeFontByLanguage self)
        {
            orig(self);
            baseFontSizes.Clear();
            RefreshAll();
        }

        private static void OnSetFontHook()
        {
            baseFontSizes.Clear();
            RefreshAll();
        }

        private static Component GetTmp(SetTextMeshProGameText self)
        {
            if (self == null || tmproField == null)
            {
                return null;
            }
            return tmproField.GetValue(self) as Component;
        }

        private static void CaptureBaseAndApply(SetTextMeshProGameText self)
        {
            var tmp = GetTmp(self);
            if (tmp == null || fontSizeProperty == null)
            {
                return;
            }
            var baseSize = (float)fontSizeProperty.GetValue(tmp);
            baseFontSizes[tmp.GetInstanceID()] = baseSize;
            ApplyScale(tmp, baseSize);
        }

        private static void ApplyScale(Component tmp, float? baseSize = null)
        {
            if (tmp == null || fontSizeProperty == null)
            {
                return;
            }
            var id = tmp.GetInstanceID();
            if (!baseSize.HasValue)
            {
                if (!baseFontSizes.TryGetValue(id, out var cached))
                {
                    return;
                }
                baseSize = cached;
            }
            var target = baseSize.Value * CustomKnight.GlobalSettings.InGameFontScale;
            fontSizeProperty.SetValue(tmp, target);
            forceMeshUpdateMethod?.Invoke(tmp, null);
        }
    }
}
