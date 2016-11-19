using System;
using System.Reflection;
using LiveSplit.LaMulanaRemake;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.LaMulanaRemake
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "La-Mulana Remake Auto Splitter"; }
        }

        public string Description
        {
            get { return "Autosplitter for La-Mulana (remake)"; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new LaMulanaComponent();
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string UpdateURL
        {
            get { return "http://livesplit.org/update/"; }
        }

        public string XMLURL
        {
            get { return "http://livesplit.org/update/Components/noupdates.xml"; }
        }
    }
}
