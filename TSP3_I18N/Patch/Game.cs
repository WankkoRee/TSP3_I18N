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
        /// <summary>
        /// 修改字体，针对静态字体
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Text))]
        [HarmonyPatch("OnEnable")]
        public static void FontPatch(Text __instance)
        {
            if (__instance.font == null)
            {
                //Plugin.Log.LogDebug("字体为 null");
                return;
            }
            string FontName = __instance.font.name;
            if (Plugin.fontsPatcherPool.Contains(FontName) || Plugin.fontsNoPatcherPool.Contains(FontName))
            {
                return;
            }
            else if (Plugin.fonts.ContainsKey(FontName))
            {
                __instance.font = Plugin.fonts[FontName].StaticFont;
            }
            else
            {
                Plugin.Log.LogWarning($"字体: {FontName} 没有映射");
                Plugin.fontsNoPatcherPool.Add(FontName);
            }
        }

        /// <summary>
        /// 只在显示语言包内语言时，修改字体，针对动态字体
        /// 并对需要倒置的文本进行倒序处理
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TMPro.TextMeshProUGUI))]
        [HarmonyPatch("InternalUpdate")]
        public static void TMPFontPatch(TMPro.TextMeshProUGUI __instance)
        {
            if (__instance.font == null)
            {
                //Plugin.Log.LogDebug("null font found!");
                return;
            }
            string FontName = __instance.font.name;
            if (Plugin.fontsPatcherPool.Contains(FontName) || Plugin.fontsNoPatcherPool.Contains(FontName))
            {
                return;
            }
            else if (Plugin.fonts.ContainsKey(FontName))
            {
                __instance.font = Plugin.fonts[FontName].DynamicFont;
            }
            else
            {
                Plugin.Log.LogWarning($"字体: {FontName} 没有映射");
                Plugin.fontsNoPatcherPool.Add(FontName);
            }

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

        /// <summary>
        /// 在资源包加载完毕时，对一些资源进行增改
        /// 使首次进入游戏的语言选择界面可以显示语言包
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssetBundle))]
        [HarmonyPatch("LoadAsset_Internal")]
        [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
        static void LoadAssetPatch(string name, Type type, AssetBundle __instance)
        {
            if (name == "assetbundlemanifest")
            {
                GameObject SubtitleSelectionGroup = GameObject.Find("Subtitle_Selection_Group");
                if (SubtitleSelectionGroup != null)
                {
                    GameObject lastLanguageButton = SubtitleSelectionGroup.transform.GetChild(SubtitleSelectionGroup.transform.childCount - 1).gameObject;
                    GameObject newLanguageButton = UnityEngine.Object.Instantiate(lastLanguageButton, SubtitleSelectionGroup.transform, false);
                    newLanguageButton.name = $"Settings_Character_Button (Mod_{newLanguageButton})";
                    newLanguageButton.transform.Find("Default").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = Plugin.newlanguageLocalName;
                    UnityEngine.UI.Toggle.ToggleEvent newLanguageButtonEvent = newLanguageButton.GetComponent<UnityEngine.UI.Toggle>().onValueChanged;
                    var lastLanguageCode = Traverse.Create(newLanguageButtonEvent)
                        .Field("m_PersistentCalls")
                        .Field("m_Calls")
                        .Property("Item", new object[]{0} )
                        .Field("m_Arguments")
                        .Field("m_IntArgument");
                    lastLanguageCode.SetValue(lastLanguageCode.GetValue<int>()+1);
                }
            }
        }
    }
}
