using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

using TSP3_I18N.Data;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using UnityEngine;

namespace TSP3_I18N
{
    [BepInPlugin("wankkoree.TSP3.i18n", "TSP3翻译插件", "1.0")]
    [BepInProcess("The Stanley Parable Ultra Deluxe.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static ConfigEntry<string> FontName;
        public static ConfigEntry<string> DictsName;
        public static string newlanguageName = "Chinese";
        public static string newlanguageCode = "zh";
        public static string newlanguageLocalName = "简体中文";

        public static Font TranslateFont;
        public static TMPro.TMP_FontAsset TMPTranslateFont;

        public static Dictionary<string, Dict> dicts = new Dictionary<string, Dict>();

        private void Awake()
        {
            Log = base.Logger;
        }
        private void Start()
        {
            FontName = Config.Bind<string>("config", "FontName", "geetype_meiheigb_flash", "put font package to <GameName>/BepInEx/plugins/TSP3_I18N");
            DictsName = Config.Bind<string>("config", "DictsName", "dicts.json", "put dicts package to <GameName>/BepInEx/plugins/TSP3_I18N");
            LoadFont(FontName.Value);
            LoadDicts(DictsName.Value);
            Harmony.CreateAndPatchAll(typeof(Patch.Util));
            Harmony.CreateAndPatchAll(typeof(Patch.Game));
            Log.LogMessage("《史丹利的寓言：终极豪华版》翻译插件 已加载");

        }

        /// <summary>
        /// 加载字体
        /// </summary>
        public void LoadFont(string fontName)
        {
            try
            {
                string path = $"{Paths.PluginPath}/TSP3_I18N/{fontName}";
                if (File.Exists(path))
                {
                    var ab = AssetBundle.LoadFromFile(path);
                    TranslateFont = ab.LoadAsset<Font>(fontName);
                    TMPTranslateFont = ab.LoadAsset<TMPro.TMP_FontAsset>($"{fontName} SDF");
                    if (TranslateFont != null && TMPTranslateFont != null)
                    {
                        Log.LogMessage($"已加载字体包: {fontName}");
                    }
                    else
                    {
                        Log.LogError($"字体包: {fontName} 已损坏, 请检查文件");
                    }
                    ab.Unload(false);
                }
                else
                {
                    Log.LogError($"字体包: {fontName} 未找到, 请检查路径: {path} 是否正确");
                }
            }
            catch (Exception e)
            {
                Log.LogError($"加载字体包失败: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 加载语言包
        /// </summary>
        public void LoadDicts(string dictsName)
        {
            try
            {
                string path = $"{Paths.PluginPath}/TSP3_I18N/{dictsName}";
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
                            Log.LogWarning($"语言包中存在重复的翻译对象: {dict.Term}");
                            dicts[dict.Term] = dict;
                        }
                    }
                    Log.LogMessage($"已加载语言包: {dictsName}");
                }
                else
                {
                    Log.LogError($"语言包: {dictsName} 未找到, 请检查路径: {path} 是否正确");
                }
            }
            catch (Exception e)
            {
                Log.LogError($"加载语言包失败: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}