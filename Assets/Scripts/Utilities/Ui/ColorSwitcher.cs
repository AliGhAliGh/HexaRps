using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Ui
{
    public class ColorSwitcher : CodeSwitcher
    {
        [SerializeField] private List<SwitchColor> colors;
        [SerializeField] private Graphic myImage;

        protected override void CodeChanged(string value)
        {
            myImage.enabled = value != null;
            myImage.color = (colors.FirstOrDefault(c => c.code == value) ?? colors.First()).color;
        }
    }

    [Serializable]
    public class SwitchColor
    {
        public Color color;
        public string code;
    }
}
