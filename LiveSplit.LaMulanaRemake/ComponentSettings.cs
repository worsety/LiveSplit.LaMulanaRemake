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

        struct MappingControlPair { public TextBox splitname; public Button mapto; };
        List<MappingControlPair> mapping_controls = new List<MappingControlPair>();

        public IDictionary<string, string> GetSplitMap(SplitMatcher remakesplitter)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (var a in mapping_controls)
                if (a.mapto.Tag != null && remakesplitter.intsplits.ContainsKey((string)a.mapto.Tag))
                    ret.Add(a.splitname.Text, (string)a.mapto.Tag);
            return ret;
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
            button.Tag = menuitem.Tag;
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

            this.SuspendLayout();

            splitCondMenu.MenuItems.Clear();
            AddSplitConds(splitCondMenu.MenuItems, remakesplitter.splitcats);

            mapping_controls.Clear();

            var listsofsplits = new[] {
                new { panel = unkSplitsLayout, splits = splitnames.Where((x) => !remakesplitter.splits.ContainsKey(x))},
                new { panel = knownSplitsLayout, splits = splitnames.Where((x) => remakesplitter.splits.ContainsKey(x))},
                new { panel = otherSplitsLayout, splits = remakesplitter.splits.Keys.Where((x) => !splitnames.Contains(x))}
            };
            foreach (var y in listsofsplits)
            {
                y.panel.Controls.Clear();
                y.panel.RowStyles.Clear();
                foreach (string split in y.splits)
                {
                    TextBox splitbox = new TextBox { Text = split, Dock = DockStyle.Fill };
                    Button selector;
                    if (remakesplitter.splits.ContainsKey(split))
                        selector = new Button
                        {
                            Text = remakesplitter.displaynames[remakesplitter.splits[split]],
                            Tag = remakesplitter.splits[split],
                            Dock = DockStyle.Fill,
                        };
                    else
                        selector = new Button { Text = "Unmapped", Tag = null };
                    selector.Dock = DockStyle.Fill;
                    selector.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    selector.Click += (object sender, EventArgs evargs) => splitCondMenu.Show((Control)sender, new System.Drawing.Point(0, 0));
                    y.panel.Controls.Add(splitbox);
                    y.panel.Controls.Add(selector);
                    mapping_controls.Add(new MappingControlPair { splitname = splitbox, mapto = selector });
                    y.panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
                y.panel.RowCount = y.panel.RowStyles.Count;
            }

            this.ResumeLayout();
        }
    }
}
