// Source: https://gist.github.com/NicolasChicunque/c2512380b1732d50e75fac4574a44b26

using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Editor.Toolbar
{
    public static class MainToolbarTimescaleSlider
    {
        private const float MinTimeScale = 0f;
        private const float MaxTimeScale = 5f;

        [MainToolbarElement("Timescale/Slider", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement TimeSlider()
        {
            MainToolbarElementStyler.StyleElement<VisualElement>("Timescale/Slider", element =>
            {
                element.style.paddingLeft = 10f;
            });

            return new MainToolbarSlider(
                new MainToolbarContent("Time Scale", "Time Scale"),
                Time.timeScale,
                MinTimeScale,
                MaxTimeScale,
                value => Time.timeScale = value);
        }
    }
}

