using System;
using System.Collections.Generic;

namespace LiveSplit.LaMulanaRemake
{
    public class RemakeSplitter : SplitMatcher
    {
        public List<string> items, events, guardians;

        // Yeah all this is pretty disgusting
        bool inshop() => (0x800000 == (game.Var<uint>("flags-4") & 0x800000));
        byte getbyte(int x) => game.Var<byte>(string.Format("byte-{0:x3}", x));
        ushort getword(int x) => game.Var<ushort>(string.Format("word-{0:x3}", x));

        public override bool StartTimer() => (0x10000 == (0x10000 & game.Var<uint>("flags-5")));
        public override bool ProvidesGameTime => true;
        public override TimeSpan? GameTime() => TimeSpan.FromMilliseconds(game.Var<uint>("igt"));

        // The byte array is saved to the save file at 0x11 and is 4kb
        // It is immediately followed by the word array (big endian in the save) which is 510 bytes (yes 255 words, not 256)
        // The inventory words are mostly 1 for possession
        // Chest item bytes become 1 when opened, 2 when acquired
        // Shop item bytes mostly become 1 when bought, 2 when you leave the shop
        // Items which are found outside of chests, e.g. weapons, just become 1?
        // Minibosses become 1 when defeated.  Some become 2 when you re-enter their room e.g. buer?
        // Guardians become 1 when their ankh (visibly) spawns, 2 when the fight starts, 3 when the fight ends

        void AddItem(string name, Func<bool> pred)
        {
            intsplits[name] = () => pred() && !inshop();
            items.Add(name);
        }

        void AddItemB(string name, int byteidx, int value = 2)
        {
            intsplits[name] = () => (getbyte(byteidx) == value && !inshop());
            items.Add(name);
        }

        void AddItemW(string name, int wordidx, int value = 1)
        {
            intsplits[name] = () => (getword(wordidx) == value && !inshop());
            items.Add(name);
        }

        void AddEvent(string name, int byteidx, int thres = 1)
        {
            intsplits[name] = () => (getbyte(byteidx) >= thres);
            events.Add(name);
        }

        void AddEvent(string name, Func<bool> pred)
        {
            intsplits[name] = pred;
            events.Add(name);
        }

        void AddGuardian(string name, int byteidx, int value = 3)
        {
            intsplits[name] = () => (getbyte(byteidx) >= value);
            guardians.Add(name);
        }

        public RemakeSplitter() : base()
        {
            game = new EyeOfTruth.LaMulanaRemake();
            items = new List<string>();
            events = new List<string>();
            guardians = new List<string>();

            AddEvent("Score=12", 0x07b, 12);
            AddItemB("deathv.exe", 0x14f);
            AddItemB("Guidance orb", 0x0c7);
            AddItemB("Grail", 0x0a9);
            AddItemW("Hand scanner", 0x014);
            AddEvent("Guidance grail", 0x064);
            AddItemW("Shurikens", 0x008);
            AddItemW("Caltrops", 0x00e);
            AddEvent("Spring grail", 0x067);
            AddEvent("Mausoleum grail", 0x065);
            AddItemB("Hermes' Boots", 0x0ba);
            AddEvent("Mausoleum jewel chest", 0x163);
            AddItemB("Guidance jewel", 0x08e);
            AddEvent("Sun grail", 0x066);
            AddItemW("Knife", 0x003);
            AddItemB("Sun orb", 0x0ca);
            AddEvent("Eden spot #1", 0x29e);
            AddItemW("Grapple claw", 0x02c);
            AddEvent("Moon grail", 0x06f);
            AddItemW("Axe", 0x005);
            AddEvent("Eden spot #2", 0x270);
            AddEvent("Anubis visit", 0x32a);
            AddItemB("Mausoleum jewel", 0x08f);
            AddEvent("Endless grail", 0x06b);
            AddEvent("Visit fairy queen", 0x1f5, 1);
            AddItemB("Mausoleum orb", 0x0c9);
            AddEvent("Sakit ankh", 0x164);
            AddItemB("Helmet", 0x0ac);
            AddItemW("Dragon Bone", 0x01b, 0x9d);
            AddEvent("Black witch", 0x1df);
            AddEvent("Nuckelavee", 0x191);
            AddItemB("Origin seal", 0x0c2);
            AddEvent("Floodgates", 0x199);
            AddItemB("Glove", 0xa8);
            AddEvent("Buer", 0x17a);
            AddItemB("Isis' Pendant", 0xaa);
            AddItemW("Flares", 0x00b);
            AddEvent("Inferno grail", 0x068);
            AddItemB("Ice cape", 0x0c1);
            AddEvent("Fairies", 0x1f5, 2);
            AddItemB("Sun jewel", 0x090);
            AddGuardian("Ellmac start", 0x0f8, 2);
            AddGuardian("Ellmac", 0x0f8);
            AddEvent("Twins grail (front)", 0x06a);
            AddItemW("Book of the dead", 0x036);
            AddGuardian("Bahamut start", 0x0f9, 2);
            AddGuardian("Bahamut", 0x0f9);
            AddGuardian("Sakit start", 0x0f7, 2);
            AddGuardian("Sakit", 0x0f7);
            AddGuardian("Baphomet start", 0x0fc, 2);
            AddGuardian("Baphomet", 0x0fc);
            AddGuardian("Viy start", 0x0fa, 2);
            AddGuardian("Viy", 0x0fa);
            AddGuardian("Amphisbaena start", 0x0f6, 2);
            AddGuardian("Amphisbaena", 0x0f6);
            AddGuardian("Tiamat start", 0x0fd, 2);
            AddGuardian("Tiamat", 0x0fd);
            AddGuardian("Palenque start", 0x0fb, 2);
            AddGuardian("Palenque", 0x0fb);
            AddGuardian("Mother start", 0x0fe, 2);
            AddGuardian("Mother", 0x0fe);
            AddEvent("Escape", () => (0x8020001 == (0x8020001 & game.Var<uint>("flags-4"))));

            AddEvent("Guidance jewel puzzle step 1", 0x136, 1);
            AddEvent("Guidance jewel puzzle step 2", 0x136, 2);
            AddEvent("Guidance jewel puzzle", 0x136, 3);
            AddEvent("Amphisbaena ankh puzzle step 1", 0x136, 2);
            AddEvent("Amphisbaena ankh puzzle", 0x136, 5);
            AddEvent("Jump into the sun puzzle", 0x17e, 2);
            AddEvent("Jumped into the sun", 0x267, 1);
            AddEvent("Grind down the watchtower step 1", 0x266, 1);
            AddEvent("Grind down the watchtower", 0x266, 2); // fixme: might include the softlock state
            AddEvent("Peryton", 0x1d8);
            AddEvent("Luck fairy", () => (0x200 == (0x200 & game.Var<uint>("flags-2"))));
            AddEvent("Fairies on cooldown", () => (0x10 == (0x10 & game.Var<uint>("flags-2"))));
            AddEvent("Fairies off cooldown", () => (0 == (0x10 & game.Var<uint>("flags-2"))));
            AddEvent("Grave lift activated", 0x249);
            AddItemB("Flail whip", 0x07e);
            AddEvent("Lit the tower of the goddess", 0x271);
            AddEvent("Left hatch", 0x194);
            AddEvent("Right hatch", 0x195);
            AddEvent("Eden", 0x226);
            AddItemB("Lamp of time", 0x9b);
            AddEvent("Recharge lamp", () => (getword(0x019) == 1));
            AddEvent("Read cogs tablets", () => (getbyte(0x23b) >= 1 && getbyte(0x23c) >= 1 && getbyte(0x23d) >= 1));

            guardians.Sort();
            splitcats.Add("Guardians", guardians);
            items.Sort();
            splitcats.Add("Items", items);
            events.Sort();
            splitcats.Add("Events", events);

            foreach (var key in intsplits.Keys)
                splits[key.Normalize().ToLowerInvariant()] = key;
        }
    }
}
