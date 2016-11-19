using System;

namespace EyeOfTruth
{
    public class LaMulanaRemake : WatchedProcess
    {
        struct Offsets
        {
            public int bytes, wordsptr, flags, igt;
        };
        Offsets? offsets;
        string version;

        public override bool Attach()
        {
            if (!Attach("LaMulanaWin"))
            {
                offsets = null;
                return false;
            }

            if (offsets != null)
                return true;

            version = proc.MainModule.FileVersionInfo.FileVersion;
            if (proc.MainModule.FileVersionInfo.FileVersion == "1.3.3.1")
                offsets = new Offsets { bytes = 0x2D7E80, wordsptr = 0x2D7858, flags = 0x2D7BC8, igt = 0x2D724C };
            if (proc.MainModule.FileVersionInfo.FileVersion == "1.5.5.2")
                offsets = new Offsets { bytes = 0x2E1E48, wordsptr = 0x2E1820, flags = 0x2E1B90, igt = 0x2E1214 };

            if (offsets == null)
                return false;

            for (int i = 0; i < 0x1000; i++)
                vars.Add(new GameVar<byte>(String.Format("byte-{0:x3}", i), offsets.Value.bytes + i));

            for (int i = 0; i < 255; i++)
                vars.Add(new GameVar<ushort>(String.Format("word-{0:x3}", i), offsets.Value.wordsptr, i * 2));

            vars.Add(new GameVar<uint>("flags-1", offsets.Value.flags));
            vars.Add(new GameVar<uint>("flags-2", offsets.Value.flags + 4));
            vars.Add(new GameVar<uint>("flags-3", offsets.Value.flags + 8));
            vars.Add(new GameVar<uint>("flags-4", offsets.Value.flags + 12));
            vars.Add(new GameVar<uint>("flags-5", offsets.Value.flags + 16));

            vars.Add(new GameVar<uint>("igt", offsets.Value.igt));
            return true;
        }
    }
}
