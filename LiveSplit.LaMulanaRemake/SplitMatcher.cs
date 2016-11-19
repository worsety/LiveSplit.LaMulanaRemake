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
        public OrderedDictionary splitcats;

        public DateTime lastsplitat = DateTime.MinValue;
        private Regex split_regex = new Regex(@"^(-|{.+})?\s*(.+?)\s*$", RegexOptions.Compiled);

        public SplitMatcher()
        {
            intsplits = new Dictionary<string, Func<bool>>();
            splits = new Dictionary<string, string>();
            splitcats = new OrderedDictionary();
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
