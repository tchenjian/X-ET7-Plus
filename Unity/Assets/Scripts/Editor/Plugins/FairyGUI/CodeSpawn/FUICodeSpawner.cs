using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ET;
using FairyGUI.Utils;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using FileMode = System.IO.FileMode;

namespace FUIEditor
{
    public enum ObjectType
    {
        None,
        graph,
        group,
        image,
        loader,
        loader3D,
        movieclip,
        textfield,
        textinput,
        richtext,
        list
    }
    
    public enum ComponentType
    {
        None,
        Component,
        Button,
        ComboBox, // 下拉框
        Label,
        ProgressBar,
        ScrollBar,
        Slider,
        Tree
    }
    
    public static class FUICodeSpawner
    {
        // 名字空间
        public static string NameSpace = "ET.Client";
        
        // 类名前缀
        public static string ClassNamePrefix = "FUI_";
        
        // 代码生成路径
        public const string FUIAutoGenDir = "../Unity/Assets/Scripts/Codes/ModelView/Client/Demo/FUIAutoGen";
        public const string ModelViewCodeDir = "../Unity/Assets/Scripts/Codes/ModelView/Client/Demo/FUI";
        public const string HotfixViewCodeDir = "../Unity/Assets/Scripts/Codes/HotfixView/Client/Demo/FUI";

        // 不生成使用默认名称的成员
        public static readonly bool IgnoreDefaultVariableName = false;
        
        public static readonly Dictionary<string, PackageInfo> PackageInfos = new Dictionary<string, PackageInfo>();

        public static readonly Dictionary<string, ComponentInfo> ComponentInfos = new Dictionary<string, ComponentInfo>();
        
        public static readonly MultiDictionary<string, string, ComponentInfo> ExportedComponentInfos = new MultiDictionary<string, string, ComponentInfo>();

        private static readonly HashSet<string> ExtralExportURLs = new HashSet<string>();

        public static void FUICodeSpawn(bool isExportFUIMultiLang, string xmlPath)
        {
            if (isExportFUIMultiLang)
            {
                if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
                {
                    Log.Error("没有提供语言文件！可查看此文档来生成：https://www.fairygui.com/docs/editor/i18n");
                    return;
                }
                
                FUILocalizeHandler.Localize(xmlPath);
            }
            
            ParseAndSpawnCode();

            AssetDatabase.Refresh();
        }

        private static void ParseAndSpawnCode()
        {
            ParseAllPackages();
            AfterParseAllPackages();
            SpawnCode();
        }

        private static void ParseAllPackages()
        {
            PackageInfos.Clear();
            ComponentInfos.Clear();
            ExportedComponentInfos.Clear();
            ExtralExportURLs.Clear();

            string fuiAssetsDir = Application.dataPath + "/../../FGUIProject/assets";
            string[] packageDirs = Directory.GetDirectories(fuiAssetsDir);
            foreach (var packageDir in packageDirs)
            {
                PackageInfo packageInfo = ParsePackage(packageDir);
                PackageInfos.Add(packageInfo.Id, packageInfo);
            }
        }

        private static PackageInfo ParsePackage(string packageDir)
        {
            PackageInfo packageInfo = new PackageInfo();

            packageInfo.Path = packageDir;
            packageInfo.Name = Path.GetFileName(packageDir);
                
            XML xml = new XML(File.ReadAllText(packageDir + "/package.xml"));
            packageInfo.Id = xml.GetAttribute("id");

            if (xml.elements[0].name != "resources" || xml.elements[1].name != "publish")
            {
                throw new Exception("package.xml 格式不对！");
            }
            
            foreach (XML element in xml.elements[0].elements)
            {
                if (element.name != "component")
                {
                    continue;
                }
                
                PackageComponentInfo packageComponentInfo = new PackageComponentInfo();
                packageComponentInfo.Id = element.GetAttribute("id");
                packageComponentInfo.Name = element.GetAttribute("name");
                packageComponentInfo.Path = "{0}{1}{2}".Fmt(packageDir, element.GetAttribute("path"), packageComponentInfo.Name);
                packageComponentInfo.Exported = element.GetAttribute("exported") == "true";
                
                packageInfo.PackageComponentInfos.Add(packageComponentInfo.Name, packageComponentInfo);

                ComponentInfo componentInfo = ParseComponent(packageInfo, packageComponentInfo);
                string key = "{0}/{1}".Fmt(componentInfo.PackageId, componentInfo.Id);
                ComponentInfos.Add(key, componentInfo);
            }

            return packageInfo;
        }

        private static ComponentInfo ParseComponent(PackageInfo packageInfo, PackageComponentInfo packageComponentInfo)
        {
            ComponentInfo componentInfo = new ComponentInfo();
            componentInfo.PackageId = packageInfo.Id;
            componentInfo.Id = packageComponentInfo.Id;
            componentInfo.Name = packageComponentInfo.Name;
            componentInfo.NameWithoutExtension = Path.GetFileNameWithoutExtension(packageComponentInfo.Name);
            componentInfo.Url = "ui://{0}{1}".Fmt(packageInfo.Id, packageComponentInfo.Id);
            componentInfo.Exported = packageComponentInfo.Exported;
            componentInfo.ComponentType = ComponentType.Component;

            XML xml = new XML(File.ReadAllText(packageComponentInfo.Path));

            if (xml.attributes.TryGetValue("extention", out var typeName))
            {
                ComponentType type = EnumHelper.FromString<ComponentType>(typeName);
                if (type == ComponentType.None)
                {
                    Debug.LogError("{0}类型没有处理！".Fmt(typeName));
                }
                else
                {
                    componentInfo.ComponentType = type;
                }
            }

            foreach (XML element in xml.elements)
            {
                if (element.name == "displayList")
                {
                    componentInfo.DisplayList = element.elements;
                }
                else if (element.name == "controller")
                {
                    componentInfo.ControllerList.Add(element);
                }
                else if (element.name == "relation")
                { 
                    
                }
                else if (element.name == "customProperty")
                { 
                    
                }
                else
                {
                    if (element.name == "ComboBox" && componentInfo.ComponentType == ComponentType.ComboBox)
                    {
                        ExtralExportURLs.Add(element.GetAttribute("dropdown"));
                    }
                }
            }

            return componentInfo;
        }
        
        // 检查哪些组件可以导出。需要在 ParseAllPackages 后执行，因为需要有全部 package 的信息。
        private static void AfterParseAllPackages()
        {
            foreach (ComponentInfo componentInfo in ComponentInfos.Values)
            {
                componentInfo.CheckCanExport(ExtralExportURLs, IgnoreDefaultVariableName);
            }
            
            foreach (ComponentInfo componentInfo in ComponentInfos.Values)
            {
                componentInfo.SetVariableInfoTypeName();
            }
        }
        
        private static void SpawnCode()
        {
            if (Directory.Exists(FUIAutoGenDir))
            {
                Directory.Delete(FUIAutoGenDir, true);
            }
            
            foreach (ComponentInfo componentInfo in ComponentInfos.Values)
            {
                FUIComponentSpawner.SpawnComponent(componentInfo);
            }
            
            foreach (var kv in ExportedComponentInfos)
            {
                FUIBinderSpawner.SpawnCodeForPanelBinder(PackageInfos[kv.Key], kv.Value);
            }

            FUIPanelIdSpawner.SpawnPanelId();
            FUIBinderSpawner.SpawnFUIBinder();

            foreach (PackageInfo packageInfo in PackageInfos.Values)
            {
                string panelName = "{0}Panel.xml".Fmt(packageInfo.Name);
                if (packageInfo.PackageComponentInfos.TryGetValue(panelName, out var packageComponentInfo))
                {
                    string componentId = $"{packageInfo.Id}/{packageComponentInfo.Id}";
                    if (ComponentInfos.TryGetValue(componentId, out var componentInfo))
                    {
                        FUIPanelSpawner.SpawnPanel(packageInfo.Name, componentInfo.NameSpace);
                        FUIPanelSystemSpawner.SpawnPanelSystem(packageInfo.Name, componentInfo);
                        FUIEventHandlerSpawner.SpawnEventHandler(packageInfo.Name);
                    }
                }
            }
        }


    }
}











