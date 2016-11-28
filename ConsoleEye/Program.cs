using System;
using System.Collections.Generic;
using EyeOfTruth;
using System.Threading;
using LiveSplit.ComponentUtil;
using System.Xml.Linq;
using System.ComponentModel;

namespace CrappyConsoleEye
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.UtcNow, now = DateTime.UtcNow;
            LaMulanaRemake game = new LaMulanaRemake();
            Dictionary<string, string> displaynames = new Dictionary<string, string>();

            try
            {
                foreach (XElement item in XElement.Load("names.xml").Descendants("item"))
                    try
                    {
                        displaynames.Add(item.Attribute("id").Value, item.Attribute("value").Value);
                    }
                    catch (ArgumentException)
                    {
                        System.Console.WriteLine("\"{0}\" is already mapped to \"{1}\", duplicate line in names.xml is trying to also map it to \"{2}\"",
                            item.Attribute("id").Value, displaynames[item.Attribute("id").Value], item.Attribute("value").Value);
                    }
            }
            catch (System.IO.FileNotFoundException) { System.Console.WriteLine("Absent names.xml"); }
            catch (System.Xml.XmlException) { System.Console.WriteLine("Malformed names.xml"); }

            MemoryWatcherList.MemoryWatcherDataChangedEventHandler changehandler =
                (MemoryWatcher w) =>
                {
                    object cur = w.Current, old = w.Old;
                    string name;
                    if (!displaynames.TryGetValue(w.Name, out name))
                        name = w.Name;
                    if (w.Name.StartsWith("flags")) // flags are interesting but noisy
                        return;
                    if (w.Name == "igt") // so very spammy
                        return;

                    string format = "";
                    if (cur is byte || cur is sbyte)
                        format = ":x2";
                    else if (cur is ushort || cur is short)
                        format = ":x4";
                    else if (cur is uint || cur is int)
                        format = ":x8";
                    System.Console.WriteLine("({0,10:f02}) {1,15} := {2" + format + "} from {3" + format + "}", (now - start).TotalSeconds, name, cur, old);
                };
            game.vars.OnWatcherDataChanged += changehandler;
            while (true)
            {
                Thread.Sleep(5);
                try
                {
                    if (!game.Attach())
                        continue;
                    now = DateTime.UtcNow;
                    game.vars.UpdateAll(game.proc);
                }
                catch (Win32Exception) { }
            }
        }
    }
}
