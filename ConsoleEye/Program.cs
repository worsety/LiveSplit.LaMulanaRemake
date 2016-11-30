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
        static Dictionary<string, string> displaynames = new Dictionary<string, string>();

        static void loadnames()
        {
            Dictionary<string, string> tempnames = new Dictionary<string, string>();

            try
            {
                foreach (XElement item in XElement.Load("names.xml").Descendants("item"))
                    try
                    {
                        tempnames.Add(item.Attribute("id").Value, item.Attribute("value").Value);
                    }
                    catch (ArgumentException)
                    {
                        System.Console.WriteLine("\"{0}\" is already mapped to \"{1}\", duplicate line in names.xml is trying to also map it to \"{2}\"",
                            item.Attribute("id").Value, tempnames[item.Attribute("id").Value], item.Attribute("value").Value);
                    }
                displaynames = tempnames;
            }
            catch (System.IO.FileNotFoundException) { System.Console.WriteLine("Absent names.xml"); }
            catch (System.IO.IOException) { System.Console.WriteLine("Error reading names.xml"); }
            catch (System.Xml.XmlException) { System.Console.WriteLine("Malformed names.xml"); }
        }

        static void changed(object cur, object old, string name, TimeSpan timestamp)
        {
            string displayname;
            if (!displaynames.TryGetValue(name, out displayname))
                displayname = name;
            if (name.StartsWith("flags")) // flags are interesting but noisy
                return;
            if (name == "igt") // so very spammy
                return;

            string format = "";
            if (cur is byte || cur is sbyte)
                format = ":x2";
            else if (cur is ushort || cur is short)
                format = ":x4";
            else if (cur is uint || cur is int)
                format = ":x8";
            System.Console.WriteLine("({0,10:f02}) {1,15} := {2" + format + "} from {3" + format + "}", timestamp.TotalSeconds, displayname, cur, old);
        }

        static void Main(string[] args)
        {
            DateTime start = DateTime.UtcNow, now = DateTime.MinValue, checkednames = DateTime.MinValue;
            LaMulanaRemake game = new LaMulanaRemake();
            System.IO.FileInfo xmlfi1 = null, xmlfi2;

            MemoryWatcherList.MemoryWatcherDataChangedEventHandler changehandler =
                (MemoryWatcher w) => changed(w.Current, w.Old, w.Name, now - start);
            game.vars.OnWatcherDataChanged += changehandler;
            byte[] oldbytes = new byte[0x1000], newbytes;
            byte[] oldwords = new byte[512], newwords;
            while (true)
            {
                Thread.Sleep(5);
                if (DateTime.UtcNow - checkednames > TimeSpan.FromSeconds(1))
                {
                    xmlfi2 = new System.IO.FileInfo("names.xml");
                    if (xmlfi1 == null || xmlfi1.LastWriteTimeUtc != xmlfi2.LastWriteTimeUtc)
                        loadnames();
                    xmlfi1 = xmlfi2;
                    checkednames = DateTime.UtcNow;
                }
                try
                {
                    if (!game.Attach())
                        continue;
                    // I knew that using the MemoryWatchers for over 4000 variables would be slow but god damn it's slow
                    // let's not do it
                    game.vars.RemoveAll(x => x.Name.StartsWith("byte") || x.Name.StartsWith("word"));
                    now = DateTime.UtcNow;
                    game.vars.UpdateAll(game.proc);
                    newbytes = game.readbytes();
                    newwords = game.readwords();
                    for (int i = 0; i < 0x1000; i++)
                        if (newbytes[i] != oldbytes[i])
                            changed(newbytes[i], oldbytes[i], String.Format("byte-{0:x3}", i), now - start);
                    for (int i = 0; i < 512; i += 2)
                    {
                        ushort oldval = BitConverter.ToUInt16(oldwords, i);
                        ushort newval = BitConverter.ToUInt16(newwords, i);
                        if (newval != oldval)
                            changed(newval, oldval, String.Format("word-{0:x3}", i >> 1), now - start);
                    }
                    oldbytes = newbytes;
                    oldwords = newwords;
                }
                catch (Win32Exception) { }
            }
        }
    }
}
