using System;
using System.Collections.Generic;
using DatastoresDX.Runtime.DataCollections;
using UICoreSystem.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    [Flags]
    public enum UIPlatform
    {
        None = 0,
        PC = 1 << 0,
        Console = 1 << 1,
        Mobile = 1 << 2,
    }
    
    public abstract class BaseViewData : DataCollectionElement
    {
        // PC/Console
        [Header("PC and Console")]
        [SerializeField, TypeReference(typeof(IView), false)]
        private TypeReference m_pcConsoleViewType;
        public TypeReference PCConsoleViewType => m_pcConsoleViewType;
        
        [SerializeField]
        private SoftRef<VisualTreeAsset> m_pcConsoleUXML;
        public SoftRef<VisualTreeAsset> PCConsoleUXML => m_pcConsoleUXML;
        
        [SerializeField]
        private SoftRef<StyleSheet> m_mouseKeyboardStyle;
        public SoftRef<StyleSheet> MouseKeyboardStyle => m_mouseKeyboardStyle;
        
        [SerializeField]
        private SoftRef<StyleSheet> m_gamepadStyle;
        public SoftRef<StyleSheet> GamepadStyle => m_gamepadStyle;
        
        // Mobile
        [Header("Mobile")]
        [SerializeField, TypeReference(typeof(IView), false)]
        private TypeReference m_mobileViewType;
        public TypeReference MobileViewType => m_mobileViewType;
        
        [SerializeField]
        private SoftRef<VisualTreeAsset> m_mobileUXML;
        public SoftRef<VisualTreeAsset> MobileUXML => m_mobileUXML;
        
        [SerializeField]
        private SoftRef<StyleSheet> m_mobileLandscapeStyle;
        public SoftRef<StyleSheet> MobileLandscapeStyle => m_mobileLandscapeStyle;
        
        [SerializeField]
        private SoftRef<StyleSheet> m_mobilePortraitStyle;
        public SoftRef<StyleSheet> MobilePortraitStyle => m_mobilePortraitStyle;
        
        public abstract Type ViewType { get; }
        
#if UNITY_EDITOR
        public override List<BundleAssetConfig> GetAssetsToBundle()
        {
            List<BundleAssetConfig> configs = new();
            
            if (!m_pcConsoleUXML.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_pcConsoleUXML.Guid,
                    Labels = new()
                    {
                        "Platform_PCConsole"
                    },
                });
            }
            if (!m_mouseKeyboardStyle.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_mouseKeyboardStyle.Guid,
                    Labels = new()
                    {
                        "Platform_PCConsole"
                    },
                });
            }
            if (!m_gamepadStyle.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_gamepadStyle.Guid,
                    Labels = new()
                    {
                        "Platform_PCConsole"
                    },
                });
            }
            
            if (!m_mobileUXML.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_mobileUXML.Guid,
                    Labels = new()
                    {
                        "Platform_Mobile"
                    },
                });
            }
            if (!m_mobileLandscapeStyle.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_mobileLandscapeStyle.Guid,
                    Labels = new()
                    {
                        "Platform_Mobile"
                    },
                });
            }
            if (!m_mobilePortraitStyle.IsNull())
            {
                configs.Add(new BundleAssetConfig()
                {
                    AssetGuid = m_mobilePortraitStyle.Guid,
                    Labels = new()
                    {
                        "Platform_Mobile"
                    },
                });
            }

            return configs;
        }

        public static string PCConsoleUXML_VarName = "m_pcConsoleUXML";
        public static string MobileUXML_VarName = "m_mobileUXML";
#endif
    }
    
    public abstract class AViewData<T> : BaseViewData where T : IView { }
}