// Source: https://gist.github.com/NicolasChicunque/c2512380b1732d50e75fac4574a44b26

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Editor.Toolbar
{
    public static class MainToolbarElementStyler
    {
        public static void StyleElement<T>(string elementName, System.Action<T> styleAction) where T : VisualElement
        {
            EditorApplication.delayCall += () =>
            {
                ApplyStyle(elementName, element =>
                {
                    var targetElement = element is T typedElement ? typedElement : element.Query<T>().First();
                    if (targetElement == null)
                    {
                        return;
                    }

                    styleAction(targetElement);
                });
            };
        }

        private static void ApplyStyle(string elementName, System.Action<VisualElement> styleCallback)
        {
            var element = FindElementByName(elementName);
            if (element == null)
            {
                return;
            }

            styleCallback(element);
        }

        private static VisualElement FindElementByName(string name)
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                var root = window.rootVisualElement;
                if (root == null)
                {
                    continue;
                }

                if (root.Q<VisualElement>(name) is { } namedElement)
                {
                    return namedElement;
                }

                if (root.Query<VisualElement>().Where(element => element.tooltip == name).First() is { } tooltipElement)
                {
                    return tooltipElement;
                }
            }

            return null;
        }
    }
}

