using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HarmonyLib;

using TSP3_I18N.Data;

using UnityEngine;
using UnityEngine.UI;

namespace TSP3_I18N.Patch
{
    class Game
    {
        public static Dictionary<int, Font> originalFont = new Dictionary<int, Font>();
        public static Dictionary<int, TMPro.TMP_FontAsset> originalTMPFont = new Dictionary<int, TMPro.TMP_FontAsset>();

        /// <summary>
        /// 只在显示语言包内语言时，修改字体，针对静态字体
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Text))]
        [HarmonyPatch("OnEnable")]
        public static void FontPatch(Text __instance)
        {
            if (__instance.font != Plugin.TranslateFont && I2.Loc.LocalizationManager.CurrentLanguage == Plugin.newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (!originalFont.ContainsKey(key))
                {
                    originalFont.Add(key, __instance.font);
                }
                __instance.font = Plugin.TranslateFont;
            }
            else if (__instance.font == Plugin.TranslateFont && I2.Loc.LocalizationManager.CurrentLanguage != Plugin.newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (originalFont.ContainsKey(key))
                {
                    __instance.font = originalFont[key];
                    originalFont.Remove(key);
                }
                else
                {
                    Plugin.Log.LogError("一个 Text 对象可能被修改过字体，但原字体未被记录");
                }
            }
        }

        /// <summary>
        /// 只在显示语言包内语言时，修改字体，针对动态字体
        /// 并对需要倒置的文本进行倒序处理
        /// 如果有不显示的文本，则设置显示方式为溢出
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TMPro.TextMeshProUGUI))]
        [HarmonyPatch("InternalUpdate")]
        public static void TMPFontPatch(TMPro.TextMeshProUGUI __instance)
        {
            if (__instance.font != Plugin.TMPTranslateFont && I2.Loc.LocalizationManager.CurrentLanguage == Plugin.newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (!originalTMPFont.ContainsKey(key))
                {
                    originalTMPFont.Add(key, __instance.font);
                }
                __instance.font = Plugin.TMPTranslateFont;
            }
            else if (__instance.font == Plugin.TMPTranslateFont && I2.Loc.LocalizationManager.CurrentLanguage != Plugin.newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (originalTMPFont.ContainsKey(key))
                {
                    __instance.font = originalTMPFont[key];
                    originalTMPFont.Remove(key);
                }
                else
                {
                    Debug.LogError("一个 Text 对象可能被修改过字体，但原字体未被记录");
                }
            }
            else if (__instance.font == Plugin.TMPTranslateFont)
            {
                Dictionary<string, ReverseText> ReverseTexts = new Dictionary<string, ReverseText>();
                foreach (Match match in Regex.Matches(__instance.text, "<(prefix|postfix) name=(.+?)>(.+?)</\\1>", RegexOptions.IgnoreCase))
                {
                    string position = match.Groups[1].Value;
                    string name = match.Groups[2].Value;
                    string value = match.Groups[3].Value;
                    if (!ReverseTexts.ContainsKey(name))
                    {
                        ReverseTexts.Add(name, new ReverseText());
                    }

                    if (position == "prefix") // 前缀
                    {
                        ReverseTexts[name].Prefix = value;
                    }
                    else if (position == "postfix") // 后缀
                    {
                        ReverseTexts[name].Postfix = value;
                    }
                    else
                    {
                        Debug.LogError($"未知的倒置文本定义: {position} 出现在: {__instance.text}");
                    }
                }
                foreach (System.Collections.Generic.KeyValuePair<string, ReverseText> kv in ReverseTexts)
                {
                    if (kv.Value.Prefix == null && kv.Value.Postfix != null)
                    {
                        // 只单独显示了后缀，直接去掉包装
                        __instance.text = __instance.text.Replace(String.Format("<{0} name={1}>{2}</{0}>", "postfix", kv.Key, kv.Value.Postfix), kv.Value.Postfix);
                    }
                    else if (kv.Value.Prefix != null && kv.Value.Postfix == null)
                    {
                        // 只单独显示了前缀，直接去掉包装
                        __instance.text = __instance.text.Replace(String.Format("<{0} name={1}>{2}</{0}>", "prefix", kv.Key, kv.Value.Prefix), kv.Value.Prefix);
                    }
                    else
                    {
                        // 将前缀替换为后缀
                        __instance.text = __instance.text.Replace(String.Format("<{0} name={1}>{2}</{0}>", "prefix", kv.Key, kv.Value.Prefix), kv.Value.Postfix);
                        // 将后缀替换为前缀
                        __instance.text = __instance.text.Replace(String.Format("<{0} name={1}>{2}</{0}>", "postfix", kv.Key, kv.Value.Postfix), kv.Value.Prefix);
                    }
                }
                if (__instance.overflowMode != TMPro.TextOverflowModes.Overflow)
                {
                    if (__instance.preferredWidth > 1 && __instance.bounds.extents == Vector3.zero)
                    {
                        __instance.overflowMode = TMPro.TextOverflowModes.Overflow;
                    }
                }
            }
        }

        /// <summary>
        ///  只在显示语言包内语言时，对资源对象进行翻译
        ///  逻辑摘录自官方的 LanguageSourceData.TryGetFallbackTranslation
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(I2.Loc.LanguageSourceData))]
        [HarmonyPatch(nameof(I2.Loc.LanguageSourceData.TryGetTranslation))]
        static bool TryGetTranslationPatch(
            string term, ref string Translation, string overrideLanguage, string overrideSpecialization, bool skipDisabled, bool allowCategoryMistmatch,
            I2.Loc.LanguageSourceData __instance, ref bool __result
            )
        {
            if (I2.Loc.LocalizationManager.CurrentLanguage != Plugin.newlanguageName.ToUpper())
            {
                return true;
            }
            if (Plugin.dicts.ContainsKey(term))
            {
                Translation = Plugin.dicts[term].Chinese;
                __result = true;
                return false;
            }
            else
            {
                I2.Loc.TermData termData = __instance.GetTermData(term);
                if (termData != null)
                {
                    Translation = termData.GetTranslation(0);
                    if (Translation == "---")
                    {
                        Translation = string.Empty;
                        __result = true;
                        return false;
                    }
                    if (!string.IsNullOrEmpty(Translation))
                    {
                        __result = true;
                        return false;
                    }
                    Translation = null;
                }
                if (__instance.OnMissingTranslation == I2.Loc.LanguageSourceData.MissingTranslationAction.ShowWarning)
                {
                    Translation = string.Format("<!-Missing Translation [{0}]-!>", term);
                    __result = true;
                    return false;
                }
                if (__instance.OnMissingTranslation == I2.Loc.LanguageSourceData.MissingTranslationAction.Fallback && termData != null)
                {
                    //return __instance.TryGetFallbackTranslation(termData, out Translation, 0, overrideSpecialization, skipDisabled);
                }
                if (__instance.OnMissingTranslation == I2.Loc.LanguageSourceData.MissingTranslationAction.Empty)
                {
                    Translation = string.Empty;
                    __result = true;
                    return false;
                }
                if (__instance.OnMissingTranslation == I2.Loc.LanguageSourceData.MissingTranslationAction.ShowTerm)
                {
                    Translation = term;
                    __result = true;
                    return false;
                }
            }
            Translation = null;
            __result = false;
            return false;
        }

        /// <summary>
        ///  添加语言包索引到 LocalizationManager.mSource.mLanguages
        ///  使 I2Loc Api 可以使用语言包
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(I2.Loc.LocalizationManager))]
        [HarmonyPatch("AddSource")]
        static void AddSourcePatch(I2.Loc.LanguageSourceData Source)
        {
            I2.Loc.LanguageData newLanguage = new I2.Loc.LanguageData();
            newLanguage.Name = Plugin.newlanguageName.ToUpper();
            newLanguage.Code = Plugin.newlanguageCode;
            newLanguage.Flags = 0;
            Source.mLanguages.Add(newLanguage);

            Plugin.Log.LogInfo($"已添加语言: {Plugin.newlanguageName} 到 LocalizationManager.mSource.mLanguages");
        }

        /// <summary>
        ///  添加语言包索引到 GameMaster.languageProfileData
        ///  使设置界面可以显示语言包
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMaster))]
        [HarmonyPatch("Awake")]
        static void SubtitleProfilePatch(GameMaster __instance)
        {
            bool needAdd = true;
            foreach (SubtitleProfile language in __instance.languageProfileData.profiles)
            {
                if (language.name == $"LangaugeProfile_{Plugin.newlanguageName}")
                    needAdd = false;
            }
            if (!needAdd) return;

            SubtitleProfile newLanguage = ScriptableObject.CreateInstance<SubtitleProfile>();
            newLanguage.name = $"LangaugeProfile_{Plugin.newlanguageName}";
            newLanguage.FontSize = 30;
            newLanguage.TextboxWidth = 2000;
            newLanguage.DescriptionKey = "Menu_Language_Self_Description";
            newLanguage.DescriptionIni2Loc = Plugin.newlanguageName;
            System.Collections.Generic.List<SubtitleProfile> newLanguages = new System.Collections.Generic.List<SubtitleProfile>(__instance.languageProfileData.profiles);
            newLanguages.Add(newLanguage);
            __instance.languageProfileData.profiles = newLanguages.ToArray();

            Plugin.Log.LogInfo($"已添加语言: {Plugin.newlanguageName} 到 GameMaster.languageProfileData");
        }
    }
}
