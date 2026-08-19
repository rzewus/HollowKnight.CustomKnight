using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomKnight
{
    internal static class FontScaleManager
    {
        private static readonly HashSet<SetTextMeshProGameText> tracked = new();
        private static readonly Dictionary<int, float> baseFontSizes = new();
        private static bool hooked;

        private static FieldInfo setTextMeshField;
        private static FieldInfo changeFontTmproField;
        private static System.Type tmpType;
        private static PropertyInfo fontSizeProperty;
        private static PropertyInfo enableAutoSizingProperty;
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
            BootstrapExisting();
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
            tracked.Clear();
            baseFontSizes.Clear();
        }

        internal static void RefreshAll()
        {
            BootstrapExisting();
            foreach (var self in tracked.ToList())
            {
                ApplyTo(self);
            }
            foreach (var fontChanger in Resources.FindObjectsOfTypeAll<ChangeFontByLanguage>())
            {
                ApplyToChangeFont(fontChanger);
            }
        }

        private static void BootstrapExisting()
        {
            foreach (var self in Resources.FindObjectsOfTypeAll<SetTextMeshProGameText>())
            {
                if (self != null)
                {
                    tracked.Add(self);
                }
            }
        }

        private static void CacheReflection()
        {
            setTextMeshField = typeof(SetTextMeshProGameText).GetField("textMesh", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            changeFontTmproField = typeof(ChangeFontByLanguage).GetField("tmpro", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            tmpType = setTextMeshField?.FieldType ?? changeFontTmproField?.FieldType;
            if (tmpType != null)
            {
                fontSizeProperty = tmpType.GetProperty("fontSize", BindingFlags.Instance | BindingFlags.Public);
                enableAutoSizingProperty = tmpType.GetProperty("enableAutoSizing", BindingFlags.Instance | BindingFlags.Public);
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
            EnsureBaseCaptured(GetTmp(self));
            ApplyTo(self);
        }

        private static void OnChangeFontByLanguageSetFont(On.ChangeFontByLanguage.orig_SetFont orig, ChangeFontByLanguage self)
        {
            orig(self);
            baseFontSizes.Clear();
            RefreshAll();
        }

        private static Component GetTmp(SetTextMeshProGameText self)
        {
            if (self == null)
            {
                return null;
            }
            if (setTextMeshField != null)
            {
                var fromField = setTextMeshField.GetValue(self) as Component;
                if (fromField != null)
                {
                    return fromField;
                }
            }
            if (tmpType != null)
            {
                return self.GetComponent(tmpType) as Component;
            }
            return null;
        }

        private static Component GetTmp(ChangeFontByLanguage self)
        {
            if (self == null || changeFontTmproField == null)
            {
                return null;
            }
            return changeFontTmproField.GetValue(self) as Component;
        }

        private static void EnsureBaseCaptured(Component tmp)
        {
            if (tmp == null || fontSizeProperty == null)
            {
                return;
            }
            var id = tmp.GetInstanceID();
            if (!baseFontSizes.ContainsKey(id))
            {
                baseFontSizes[id] = (float)fontSizeProperty.GetValue(tmp);
            }
        }

        private static void ApplyTo(SetTextMeshProGameText self)
        {
            ApplyToComponent(GetTmp(self));
        }

        private static void ApplyToChangeFont(ChangeFontByLanguage self)
        {
            ApplyToComponent(GetTmp(self));
        }

        private static void ApplyToComponent(Component tmp)
        {
            if (tmp == null || fontSizeProperty == null)
            {
                return;
            }
            var id = tmp.GetInstanceID();
            if (!baseFontSizes.TryGetValue(id, out var baseSize))
            {
                baseSize = (float)fontSizeProperty.GetValue(tmp);
                baseFontSizes[id] = baseSize;
            }
            enableAutoSizingProperty?.SetValue(tmp, false);
            fontSizeProperty.SetValue(tmp, baseSize * CustomKnight.GlobalSettings.InGameFontScale);
            forceMeshUpdateMethod?.Invoke(tmp, null);
        }
    }
}
