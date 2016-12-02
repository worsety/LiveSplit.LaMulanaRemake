using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.LaMulanaRemake
{
    public class SplitMatcher
    {
        public EyeOfTruth.WatchedProcess game;

        public virtual bool StartTimer() => false;
        public virtual bool ProvidesGameTime => false;
        public virtual TimeSpan? GameTime() => null;

        public IDictionary<string, Func<bool>> intsplits;
        public IDictionary<string, string> splits;
        public IDictionary<string, string> displaynames;
        public OrderedDictionary splitcats;

        public DateTime lastsplitat = DateTime.MinValue;
        private Regex split_regex = new Regex(@"^(-|{.+})?\s*(.+?)\s*$", RegexOptions.Compiled);

        public SplitMatcher()
        {
            intsplits = new Dictionary<string, Func<bool>>();
            splits = new Dictionary<string, string>();
            splitcats = new OrderedDictionary();
            displaynames = new Dictionary<string, string>();
            splitcats.Add("Unmapped", null);
            category_stack.Push(splitcats);
        }

        // used in initialisation only
        protected Stack<OrderedDictionary> category_stack = new Stack<OrderedDictionary>();
        private Stack<string> prefix = new Stack<string>();

        protected void StartCat(string name)
        {
            var cat = new OrderedDictionary();
            category_stack.Peek().Add(name, cat);
            category_stack.Push(cat);
            prefix.Push(name);
        }

        protected void AddCond(Func<bool> pred, string intname, string displayname = null)
        {
            intsplits.Add(intname, pred);
            category_stack.Peek().Add(displayname ?? intname, intname);
            displaynames.Add(intname, (prefix.Count > 0 ? string.Join("/", prefix.Reverse()) + "/" : "") + (displayname ?? intname));
        }

        protected void EndCat()
        {
            category_stack.Pop();
            prefix.Pop();
        }

        public virtual void Update(LiveSplitState state, TimerModel timer)
        {
            if (!game.Attach())
                return;
            switch (state.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    if (StartTimer())
                        timer.Start();
                    return;
                case TimerPhase.Running:
                case TimerPhase.Paused:
                    if (ProvidesGameTime)
                    {
                        TimeSpan? igt = GameTime();
                        if (!state.IsGameTimeInitialized)
                            timer.InitializeGameTime();
                        state.IsGameTimePaused = true;
                        state.SetGameTime(igt);
                    }

                    Match m = split_regex.Match(state.CurrentSplit.Name);
                    string splitname = m.Success ? m.Groups[2].Value.Normalize().ToLowerInvariant() : "";
                    if (!splits.ContainsKey(splitname))
                    {
                        timer.SkipSplit();
                        lastsplitat = DateTime.UtcNow;
                    }
                    else if (intsplits[splits[splitname]]())
                    {
                        if (DateTime.Now - lastsplitat < TimeSpan.FromMilliseconds(100))
                            timer.SkipSplit();
                        else
                            timer.Split();
                        lastsplitat = DateTime.UtcNow;
                    }
                    return;
            }
        }
    }
}
