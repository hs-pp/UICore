using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyMVVM;
using UnityEngine;

namespace UICoreSystem.UIWorkshop
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewModelMockAttribute : Attribute
    {
        public Type ViewModelType { get; }
        public string DisplayName { get; }
        
        public ViewModelMockAttribute(Type viewModelType, string displayName)
        {
            ViewModelType = viewModelType;
            DisplayName = displayName;
        }
    }
    
    public static class ViewModelMockProvider
    {
        #region Mocks
        
        // [ViewModelMock(typeof(RemedyConsoleViewModel), "Basic")]
        // private static ViewModel CreateDefaultRemedyConsoleViewModel()
        // {
        //     return new RemedyConsoleViewModel(new RemedyConsoleModel());
        // }
        
        #endregion
        
        
        private static string NONE_NAME = "--none--";
        private static string PARAMETERLESS_CONSTRUCTOR_NAME = "Parameterless Constructor";
        
        private static Dictionary<Type, Dictionary<string, MethodInfo>> MockMethodLookup = new();
        private static Dictionary<Type, bool> HasParameterlessConstructorLookup = new();
        
        static ViewModelMockProvider()
        {
            CollectMockMethods();
        }

        private static void CollectMockMethods()
        {
            MockMethodLookup.Clear();
            HasParameterlessConstructorLookup.Clear();
            
            // Get all methods from this class that meet the following conditions:
            // - is Static
            // - is Private
            // - has zero parameters
            // - has the ViewModelMock attribute
            List<MethodInfo> methods = typeof(ViewModelMockProvider)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<ViewModelMockAttribute>() != null && m.GetParameters().Length == 0)
                .ToList();

            foreach (MethodInfo methodInfo in methods)
            {
                ViewModelMockAttribute attribute = methodInfo.GetCustomAttribute<ViewModelMockAttribute>();
                if (!MockMethodLookup.ContainsKey(attribute.ViewModelType))
                {
                    MockMethodLookup.Add(attribute.ViewModelType, new());
                }

                if (MockMethodLookup[attribute.ViewModelType].ContainsKey(attribute.DisplayName))
                {
                    Debug.LogError($"[ViewModelMockProvider] Found duplicate method name! Type: {attribute.ViewModelType.Name} Method: {attribute.DisplayName}");
                    continue;
                }

                MockMethodLookup[attribute.ViewModelType].Add(attribute.DisplayName, methodInfo);
            }
        }

        public static List<string> GetMockMethodNames(Type viewModelType)
        {
            if (viewModelType == null)
            {
                return new() { NONE_NAME };
            }
            
            List<string> methods = new();

            if (!HasParameterlessConstructorLookup.ContainsKey(viewModelType))
            {
                bool hasParameterlessConstructor = viewModelType.GetConstructor(Type.EmptyTypes) != null;
                HasParameterlessConstructorLookup.Add(viewModelType, hasParameterlessConstructor);
            }
            if (HasParameterlessConstructorLookup[viewModelType])
            {
                methods.Add(PARAMETERLESS_CONSTRUCTOR_NAME);
            }

            if (MockMethodLookup.ContainsKey(viewModelType))
            {
                methods.AddRange(MockMethodLookup[viewModelType].Keys);
            }

            return methods;
        }
        
        public static ViewModel GetMockWithMethod(Type viewModelType, string methodName)
        {
            if (methodName == NONE_NAME)
            {
                return null;
            }
            
            if (methodName == PARAMETERLESS_CONSTRUCTOR_NAME)
            {
                return DefaultParameterlessConstructor(viewModelType);
            }

            if (MockMethodLookup.ContainsKey(viewModelType))
            {
                if (MockMethodLookup[viewModelType].ContainsKey(methodName))
                {
                    return MockMethodLookup[viewModelType][methodName].Invoke(null, new object[0]) as ViewModel;
                }
            }
            
            return null;
        }

        private static ViewModel DefaultParameterlessConstructor(Type viewModelType)
        {
            return Activator.CreateInstance(viewModelType) as ViewModel;
        }
    }
}