using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheStanleyParableUltraDeluxe_Chinese
{
    [BepInPlugin("TheStanleyParableUltraDeluxe_Chinese", "TheStanleyParableUltraDeluxe_Chinese", "1.0")]
    [BepInProcess("The Stanley Parable Ultra Deluxe.exe")]
    public class TheStanleyParableUltraDeluxe_Chinese : BaseUnityPlugin
    {
        public static TheStanleyParableUltraDeluxe_Chinese Inst;
        public static Font TranslateFont;
        public static TMP_FontAsset TMPTranslateFont;
        public static ConfigEntry<string> FontName;
        public static ConfigEntry<string> DictsName;
        public static string newlanguageName = "Chinese";
        public static string newlanguageCode = "zh";
        public static System.Collections.Generic.Dictionary<int, Font> originalFont = new System.Collections.Generic.Dictionary<int, Font>();
        public static System.Collections.Generic.Dictionary<int, TMP_FontAsset> originalTMPFont = new System.Collections.Generic.Dictionary<int, TMP_FontAsset>();
        public static System.Collections.Generic.Dictionary<string, Dict> dicts = new System.Collections.Generic.Dictionary<string, Dict>();

        private void Start()
        {
            Inst = this;
            FontName = Config.Bind<string>("config", "FontName", "geetype_meiheigb_flash", "put font package to <GameName>/BepInEx/plugins/TheStanleyParableUltraDeluxe_Chinese");
            DictsName = Config.Bind<string>("config", "DictsName", "dicts.json", "put dicts package to <GameName>/BepInEx/plugins/TheStanleyParableUltraDeluxe_Chinese");
            LoadFont(FontName.Value);
            LoadDicts(DictsName.Value);
            Harmony.CreateAndPatchAll(typeof(TheStanleyParableUltraDeluxe_Chinese));
            Logger.LogInfo("《史丹利的寓言：终极豪华版》翻译插件已加载");
        }

        public void LoadDicts(string dictsName)
        {
            try
            {
                string path = $"{Paths.PluginPath}/TheStanleyParableUltraDeluxe_Chinese/{dictsName}";
                if (File.Exists(path))
                {
                    Dict[] dictsJson;
                    DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(Dict[]));
                    using (FileStream fs = File.Open(path, FileMode.Open))
                    {
                        fs.Position = 0;
                        dictsJson = (Dict[])deseralizer.ReadObject(fs);
                    }
                    foreach (Dict dict in dictsJson)
                    {
                        if (!dicts.ContainsKey(dict.Term))
                            dicts.Add(dict.Term, dict);
                        else
                        {
                            Debug.LogWarning($"语言包中存在重复的翻译对象: {dict.Term}");
                            dicts[dict.Term] = dict;
                        }
                    }
                }
                else
                {
                    Logger.LogError($"语言包: {dictsName} 未找到, 请检查路径: {path} 是否正确");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"加载语言包失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 加载字体
        /// </summary>
        /// <param name="fontName">字体名称</param>
        public void LoadFont(string fontName)
        {
            try
            {
                string path = $"{Paths.PluginPath}/TheStanleyParableUltraDeluxe_Chinese/{fontName}";
                if (File.Exists(path))
                {
                    var ab = AssetBundle.LoadFromFile(path);
                    TranslateFont = ab.LoadAsset<Font>(fontName);
                    TMPTranslateFont = ab.LoadAsset<TMP_FontAsset>($"{fontName} SDF");
                    if (TranslateFont != null && TMPTranslateFont != null)
                    {
                        Logger.LogInfo($"已加载字体包: {fontName}");
                    }
                    else
                    {
                        Logger.LogError($"字体包: {fontName} 已损坏, 请检查文件");
                    }
                    ab.Unload(false);
                }
                else
                {
                    Logger.LogError($"字体包: {fontName} 未找到, 请检查路径: {path} 是否正确");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"加载字体包失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 只在显示目标语言时修改字体
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(Text), "OnEnable")]
        public static void FontPatch(Text __instance)
        {
            if (__instance.font != TranslateFont && I2.Loc.LocalizationManager.CurrentLanguage == newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (!originalFont.ContainsKey(key))
                {
                    originalFont.Add(key, __instance.font);
                }
                __instance.font = TranslateFont;
            }
            else if (__instance.font == TranslateFont && I2.Loc.LocalizationManager.CurrentLanguage != newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (originalFont.ContainsKey(key))
                {
                    __instance.font = originalFont[key];
                    originalFont.Remove(key);
                }
                else
                {
                    Debug.LogError("一个 Text 对象可能被修改过字体，但原字体未被记录");
                }
            }
        }

        /// <summary>
        /// 只在显示目标语言时修改字体，如果有不显示的文本，则设置显示方式为溢出
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "InternalUpdate")]
        public static void TMPFontPatch(TextMeshProUGUI __instance)
        {
            if (__instance.font != TMPTranslateFont && I2.Loc.LocalizationManager.CurrentLanguage == newlanguageName.ToUpper())
            {
                int key = __instance.GetHashCode();
                if (!originalTMPFont.ContainsKey(key))
                {
                    originalTMPFont.Add(key, __instance.font);
                }
                __instance.font = TMPTranslateFont;
            }
            else if (__instance.font == TMPTranslateFont && I2.Loc.LocalizationManager.CurrentLanguage != newlanguageName.ToUpper())
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
            else if (__instance.font == TMPTranslateFont)
            {
                System.Collections.Generic.Dictionary<string, ReverseText> ReverseTexts = new System.Collections.Generic.Dictionary<string, ReverseText>();
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
                foreach(System.Collections.Generic.KeyValuePair<string, ReverseText> kv in ReverseTexts)
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
                if (__instance.overflowMode != TextOverflowModes.Overflow)
                {
                    if (__instance.preferredWidth > 1 && __instance.bounds.extents == Vector3.zero)
                    {
                        __instance.overflowMode = TextOverflowModes.Overflow;
                    }
                }
            }
        }

        /// <summary>
        ///  当使用目标语言时，对资源对象进行翻译，逻辑摘录自官方的 LanguageSourceData.TryGetFallbackTranslation
        /// </summary>
        /// <param name="term"></param>
        /// <param name="Translation"></param>
        /// <param name="overrideLanguage"></param>
        /// <param name="overrideSpecialization"></param>
        /// <param name="skipDisabled"></param>
        /// <param name="allowCategoryMistmatch"></param>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPrefix, HarmonyPatch(typeof(I2.Loc.LanguageSourceData), nameof(I2.Loc.LanguageSourceData.TryGetTranslation))]
        static bool TryGetTranslationPatch(
            string term, ref string Translation, string overrideLanguage, string overrideSpecialization, bool skipDisabled, bool allowCategoryMistmatch,
            I2.Loc.LanguageSourceData __instance, ref bool __result
            )
        {
            if (I2.Loc.LocalizationManager.CurrentLanguage != newlanguageName.ToUpper())
            {
                return true;
            }
            if (dicts.ContainsKey(term))
            {
                Translation = dicts[term].Chinese;
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

        [HarmonyPrefix, HarmonyPatch(typeof(I2.Loc.LocalizationManager), "AddSource")]
        static void AddSourcePatch(I2.Loc.LanguageSourceData Source)
        {
            I2.Loc.LanguageData newLanguage = new I2.Loc.LanguageData();
            newLanguage.Name = newlanguageName.ToUpper();
            newLanguage.Code = newlanguageCode;
            newLanguage.Flags = 0;
            Source.mLanguages.Add(newLanguage);
            Debug.Log($"已添加语言: {newlanguageName} 到 LocalizationManager.mSource.mLanguages");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GameMaster), "Awake")]
        static void SubtitleProfilePatch(GameMaster __instance)
        {
            bool needAdd = true;
            foreach (SubtitleProfile language in __instance.languageProfileData.profiles)
            {
                if (language.name == $"LangaugeProfile_{newlanguageName}")
                    needAdd = false;
            }
            if (!needAdd) return;

            SubtitleProfile newLanguage = ScriptableObject.CreateInstance<SubtitleProfile>();
            newLanguage.name = $"LangaugeProfile_{newlanguageName}";
            newLanguage.FontSize = 30;
            newLanguage.TextboxWidth = 2000;
            newLanguage.DescriptionKey = "Menu_Language_Self_Description";
            newLanguage.DescriptionIni2Loc = newlanguageName;
            System.Collections.Generic.List<SubtitleProfile> newLanguages = new System.Collections.Generic.List<SubtitleProfile>(__instance.languageProfileData.profiles);
            newLanguages.Add(newLanguage);
            __instance.languageProfileData.profiles = newLanguages.ToArray();

            Debug.Log($"已添加语言: {newlanguageName} 到 GameMaster.languageProfileData");
        }
    }
}