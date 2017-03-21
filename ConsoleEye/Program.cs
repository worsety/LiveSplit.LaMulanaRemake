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
        static Dictionary<string, string> remakenames = new Dictionary<string, string>();
        static Dictionary<string, string> classicnames = new Dictionary<string, string>();

        static Dictionary<string, string> loadnames(String fn)
        {
            Dictionary<string, string> tempnames = new Dictionary<string, string>();

            try
            {
                foreach (XElement item in XElement.Load(fn).Descendants("item"))
                    try
                    {
                        tempnames.Add(item.Attribute("id").Value, item.Attribute("value").Value);
                    }
                    catch (ArgumentException)
                    {
                        System.Console.WriteLine("\"{0}\" is already mapped to \"{1}\", duplicate line in names.xml is trying to also map it to \"{2}\"",
                            item.Attribute("id").Value, tempnames[item.Attribute("id").Value], item.Attribute("value").Value);
                    }
                return tempnames;
            }
            catch (System.IO.FileNotFoundException) { System.Console.WriteLine("Absent " + fn); }
            catch (System.IO.IOException) { System.Console.WriteLine("Error reading " + fn); }
            catch (System.Xml.XmlException) { System.Console.WriteLine("Malformed " + fn); }
            return new Dictionary<string, string>();
        }

        static void changed(object cur, object old, string name, string displayname, TimeSpan timestamp)
        {
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

        static void changedr(object cur, object old, string name, TimeSpan timestamp)
        {
            string displayname;
            if (!remakenames.TryGetValue(name, out displayname))
                displayname = name;
            changed(cur, old, name, displayname, timestamp);
        }

        static void changedc(object cur, object old, string name, TimeSpan timestamp)
        {
            string displayname;
            if (!classicnames.TryGetValue(name, out displayname))
                displayname = name;
            changed(cur, old, name, displayname, timestamp);
        }

        static bool warnedaboutaccess = false;
        static void Main(string[] args)
        {
            DateTime start = DateTime.UtcNow, now = DateTime.MinValue, checkednames = DateTime.MinValue;
            LaMulanaRemake remake = new LaMulanaRemake();
            LaMulanaClassic classic = new LaMulanaClassic();
            System.IO.FileInfo rxmlfi1 = null, rxmlfi2, cxmlfi1 = null, cxmlfi2;

            MemoryWatcherList.MemoryWatcherDataChangedEventHandler changerhandler =
                (MemoryWatcher w) => changedr(w.Current, w.Old, w.Name, now - start);
            remake.vars.OnWatcherDataChanged += changerhandler;
            MemoryWatcherList.MemoryWatcherDataChangedEventHandler changechandler =
                (MemoryWatcher w) => changedc(w.Current, w.Old, w.Name, now - start);
            classic.vars.OnWatcherDataChanged += changechandler;

            byte[] rbytes_old = new byte[0x1000], rbytes_new;
            byte[] rwords_old = new byte[510], rwords_new;

            byte[] cflags_old = new byte[875], cflags_new;
            byte[] croms_old = new byte[1344], croms_new;

            while (true)
            {
                Thread.Sleep(5);
                if (DateTime.UtcNow - checkednames > TimeSpan.FromSeconds(1))
                {
                    rxmlfi2 = new System.IO.FileInfo("names.xml");
                    if (rxmlfi1 == null || rxmlfi1.LastWriteTimeUtc != rxmlfi2.LastWriteTimeUtc)
                        remakenames = loadnames("names.xml");
                    cxmlfi2 = new System.IO.FileInfo("cnames.xml");
                    if (cxmlfi1 == null || cxmlfi1.LastWriteTimeUtc != cxmlfi2.LastWriteTimeUtc)
                        classicnames = loadnames("cnames.xml");
                    cxmlfi1 = cxmlfi2;
                    checkednames = DateTime.UtcNow;
                }
                try
                {
                    now = DateTime.UtcNow;
                    if (remake.Attach())
                    {
                        // I knew that using the MemoryWatchers for over 4000 variables would be slow but god damn it's slow
                        // let's not do it
                        remake.vars.RemoveAll(x => x.Name.StartsWith("byte") || x.Name.StartsWith("word"));
                        remake.vars.UpdateAll(remake.proc);
                        rbytes_new = remake.readbytes();
                        rwords_new = remake.readwords();
                        for (int i = 100; i < 0x1000; i++)
                            if (rbytes_new[i] != rbytes_old[i])
                                changedr(rbytes_new[i], rbytes_old[i], String.Format("byte-{0:x3}", i), now - start);
                        for (int i = 0; i < 510; i += 2)
                        {
                            ushort oldval = BitConverter.ToUInt16(rwords_old, i);
                            ushort newval = BitConverter.ToUInt16(rwords_new, i);
                            if (newval != oldval)
                                changedr(newval, oldval, String.Format("word-{0:x3}", i >> 1), now - start);
                        }
                        rbytes_old = rbytes_new;
                        rwords_old = rwords_new;
                    }
                    else if (classic.Attach())
                    {
                        classic.vars.UpdateAll(classic.proc);
                        if ((int)classic.vars["igt"].Current <= 0)
                            continue;
                        cflags_new = classic.readflags();
                        croms_new = classic.readroms();
                        for (int i = 5; i < 875; i++)
                            if (cflags_new[i] != cflags_old[i])
                                for (int j = 0; j < 8; j++)
                                    if (((cflags_new[i] ^ cflags_old[i]) & 1 << j) != 0)
                                        changedc((cflags_new[i] >> j & 1) != 0, (cflags_old[i] >> j &1) != 0, String.Format("f-{0:x3}-{1}", i, j), now - start);
                        for (int i = 0; i < 1344; i += 4)
                        {
                            if ((i & 4) != 0)
                                continue;
                            uint oldval = BitConverter.ToUInt32(croms_old, i);
                            uint newval = BitConverter.ToUInt32(croms_new, i);
                            if (newval != oldval)
                                changedc(newval, oldval, String.Format("rom-{0:x3}", i), now - start);
                        }
                        cflags_old = cflags_new;
                        croms_old = croms_new;
                    }
                }
                catch (Win32Exception e) {
                    if (e.NativeErrorCode == 5 && e.TargetSite.ToString().StartsWith("Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess"))
                    {
                        if (!warnedaboutaccess)
                            System.Console.WriteLine("Unable to access LaMulanaWin.exe, please check the compatibility settings\n"
                                + "and uncheck \"Run this program as an administrator\" if it is checked.");
                        warnedaboutaccess = true;
                    }
                }
                catch (ArgumentNullException) { }
            }
        }
    }
}
