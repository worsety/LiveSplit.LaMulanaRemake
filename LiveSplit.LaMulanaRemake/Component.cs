using System;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Collections.Generic;
using System.Xml;

namespace LiveSplit.LaMulanaRemake
{
    public class LMRComponent : IComponent
    {
        public string ComponentName
        {
            get { return "La-Mulana Remake Auto Splitter"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; set; }

        public float HorizontalWidth => 0;
        public float VerticalHeight => 0;
        public float MinimumWidth => 0;
        public float MinimumHeight => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;
        public float PaddingTop => 0;
        public float PaddingBottom => 0;

        public LMRSplitter autosplitter;

        public void Dispose() { }

        public LMRComponent()
        {

        }

        public void DrawHorizontal(System.Drawing.Graphics g, LiveSplitState state, float height, System.Drawing.Region clipRegion) { }

        public void DrawVertical(System.Drawing.Graphics g, LiveSplitState state, float width, System.Drawing.Region clipRegion) { }

        public XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("x");
        }

        public System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(XmlNode settings)
        {
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            autosplitter.Update(state);
        }
    }
}