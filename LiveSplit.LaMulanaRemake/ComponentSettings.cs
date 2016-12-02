// WARNING WARNING WARNING

// I absolutely hate GUI programming and everything I try to do turns into a horrible hack

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using LiveSplit.Model;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections;

namespace LiveSplit.LaMulanaRemake
{
    public partial class ComponentSettings : UserControl
    {
        LaMulanaComponent component;
        List<string> splitnames = new List<string>();
        private Regex split_regex = new Regex(@"^(-|{.+})?\s*(.+?)\s*$", RegexOptions.Compiled);

        public ComponentSettings(LaMulanaComponent component)
        {
            this.component = component;
            Dock = DockStyle.Fill;
            InitializeComponent();
        }

        string GetMenuPath(MenuItem m)
        {
            if (m.Parent is MenuItem)
                return GetMenuPath((MenuItem)m.Parent) + "/" + m.Text;
            return m.Text;
        }

        void SplitCondMenuHandler(object sender, EventArgs evargs)
        {
            MenuItem menuitem = (MenuItem)sender;
            Button button = (Button)menuitem.GetContextMenu().SourceControl;
            button.Text = GetMenuPath((MenuItem)sender);
            if (menuitem.Tag != null)
                component.remake.splits[(string)button.Tag] = (string)menuitem.Tag;
            else
                component.remake.splits.Remove((string)button.Tag);
        }

        void AddSplitConds(Menu.MenuItemCollection menuitems, OrderedDictionary conds)
        {
            foreach (DictionaryEntry splitcond in conds)
            {
                MenuItem item = new MenuItem((string)splitcond.Key);
                if (splitcond.Value is OrderedDictionary)
                {
                    AddSplitConds(item.MenuItems, (OrderedDictionary)splitcond.Value);
                }
                else
                {
                    item.Click += SplitCondMenuHandler;
                    item.Tag = splitcond.Value;
                }
                menuitems.Add(item);
            }
        }

        public void SetSplits(LiveSplitState state, SplitMatcher remakesplitter)
        {
            splitnames.Clear();
            foreach (ISegment segment in state.Run)
            {
                Match m = split_regex.Match(segment.Name);
                if (!m.Success)
                    continue;
                string splitname = m.Success ? m.Groups[2].Value.Normalize().ToLowerInvariant() : "";
                if (!splitnames.Contains(splitname))
                    splitnames.Add(splitname);
            }

            SuspendLayout();

            splitCondMenu.MenuItems.Clear();
            AddSplitConds(splitCondMenu.MenuItems, remakesplitter.splitcats);

            var listsofsplits = new[] {
                new { panel = unkSplitsLayout, splits = splitnames.Where((x) => !remakesplitter.splits.ContainsKey(x))},
                new { panel = knownSplitsLayout, splits = splitnames.Where((x) => remakesplitter.splits.ContainsKey(x))},
                new { panel = otherSplitsLayout, splits = remakesplitter.splits.Keys.Where((x) => !splitnames.Contains(x))}
            };
            foreach (var y in listsofsplits)
            {
                y.panel.AutoScroll = false; // If I don't do this, the layout never shrinks.  I don't even.
                y.panel.Controls.Clear();
                y.panel.RowStyles.Clear();
                foreach (string split in y.splits)
                {
                    TextBox splitbox = new TextBox { Text = split, Dock = DockStyle.Fill, ReadOnly = true };
                    Button selector;
                    selector = new Button
                    {
                        Text = remakesplitter.splits.ContainsKey(split) ? remakesplitter.displaynames[remakesplitter.splits[split]] : "Unmapped",
                        Tag = split,
                        Dock = DockStyle.Fill,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    };
                    selector.Click += (object sender, EventArgs evargs) => splitCondMenu.Show((Control)sender, new System.Drawing.Point(0, 0));
                    y.panel.Controls.Add(splitbox);
                    y.panel.Controls.Add(selector);
                    y.panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
                y.panel.RowCount = y.panel.RowStyles.Count;
                y.panel.AutoScroll = true;
            }

            ResumeLayout();
        }
    }
}
