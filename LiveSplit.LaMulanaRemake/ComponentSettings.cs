// WARNING WARNING WARNING

// I absolutely hate GUI programming and everything I try to do turns into a horrible hack

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using LiveSplit.Model;
using System.Text.RegularExpressions;

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

        struct MappingControlPair { public TextBox splitname; public ComboBox mapto; };
        List<MappingControlPair> mapping_controls = new List<MappingControlPair>();

        public IDictionary<string, string> GetSplitMap(SplitMatcher remakesplitter)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (var a in mapping_controls)
                if (remakesplitter.intsplits.ContainsKey(a.mapto.SelectedItem.ToString()))
                    ret.Add(a.splitname.Text, a.mapto.SelectedItem.ToString());
            return ret;
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

            List<string> splitlist = new List<string>();
            splitlist.Add("");
            foreach (string cat in remakesplitter.splitcats.Keys)
            {
                splitlist.Add(String.Format("--{0}--", cat));
                foreach (string split in (List<string>)remakesplitter.splitcats[cat])
                    splitlist.Add(split);
            }

            this.SuspendLayout();

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
                    int idx = remakesplitter.splits.ContainsKey(split) ? splitlist.IndexOf(remakesplitter.splits[split]) : 0;
                    TextBox splitbox = new TextBox { Text = split, Dock = DockStyle.Fill };
                    ComboBox selector = new ComboBox
                    {
                        BindingContext = new BindingContext(),
                        DataSource = splitlist,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        SelectedIndex = idx,
                        Dock = DockStyle.Fill
                    };
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
