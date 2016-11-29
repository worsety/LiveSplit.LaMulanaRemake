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
            intsplits.Add(name, () => pred() && !inshop());
            items.Add(name);
        }

        void AddItemB(string name, int byteidx, int value = 2)
        {
            AddItem(name, () => (getbyte(byteidx) == value && !inshop()));
        }

        void AddItemW(string name, int wordidx, int value = 1)
        {
            AddItem(name, () => (getword(wordidx) == value && !inshop()));
        }

        void AddEvent(string name, Func<bool> pred)
        {
            intsplits.Add(name, pred);
            events.Add(name);
        }

        void AddEvent(string name, int byteidx, int thres = 1)
        {
            AddEvent(name, () => (getbyte(byteidx) >= thres));
        }

        void AddGuardian(string name, int byteidx, int value = 3)
        {
            intsplits.Add(name, () => (getbyte(byteidx) >= value));
            guardians.Add(name);
        }

        public RemakeSplitter() : base()
        {
            game = new EyeOfTruth.LaMulanaRemake();
            items = new List<string>();
            events = new List<string>();
            guardians = new List<string>();

            AddGuardian("Amphisbaena start", 0x0f6, 2);
            AddGuardian("Amphisbaena", 0x0f6);
            AddGuardian("Sakit start", 0x0f7, 2);
            AddGuardian("Sakit", 0x0f7);
            AddGuardian("Ellmac start", 0x0f8, 2);
            AddGuardian("Ellmac", 0x0f8);
            AddGuardian("Bahamut start", 0x0f9, 2);
            AddGuardian("Bahamut", 0x0f9);
            AddGuardian("Viy start", 0x0fa, 2);
            AddGuardian("Viy", 0x0fa);
            AddGuardian("Baphomet start", 0x0fc, 2);
            AddGuardian("Baphomet", 0x0fc);
            AddGuardian("Palenque start", 0x0fb, 2);
            AddGuardian("Palenque", 0x0fb);
            AddGuardian("Tiamat start", 0x0fd, 2);
            AddGuardian("Tiamat", 0x0fd);
            AddGuardian("Mother start", 0x0fe, 2);
            AddGuardian("Mother", 0x0fe);
            AddEvent("Escape", () => (0x8020001 == (0x8020001 & game.Var<uint>("flags-4"))));

            AddEvent("Grail point: Gate of Guidance", 0x064);
            AddEvent("Grail point: Mausoleum of the Giants", 0x065);
            AddEvent("Grail point: Temple of the Sun", 0x066);
            AddEvent("Grail point: Spring in the Sky", 0x067);
            AddEvent("Grail point: Inferno Cavern", 0x068);
            AddEvent("Grail point: Chamber of Extinction", 0x069);
            AddEvent("Grail point: Twin Labyrinths (front)", 0x06a);
            AddEvent("Grail point: Endless Corridor", 0x06b);
            AddEvent("Grail point: Shrine of the Mother", 0x06c);
            AddEvent("Grail point: Gate of Illusion", 0x06d);
            AddEvent("Grail point: Graveyard of the Giants", 0x06e);
            AddEvent("Grail point: Temple of the Moon", 0x06f);
            AddEvent("Grail point: Tower of the Goddess", 0x070);
            AddEvent("Grail point: Tower of Ruin", 0x071);
            AddEvent("Grail point: Chamber of Birth", 0x072);
            AddEvent("Grail point: Twin Labyrinths (back)", 0x073);
            AddEvent("Grail point: Dimensional Corridor", 0x074);
            AddEvent("Grail point: True Shrine of the Mother", 0x075);

            AddItemB("Ankh jewel: Gate of Guidance", 0x08e);
            AddItemB("Ankh jewel: Mausoleum of the Giants", 0x08f);
            AddItemB("Ankh jewel: Temple of the Sun", 0x090);
            AddItemB("Ankh jewel: Spring in the Sky", 0x091);
            AddItemB("Ankh jewel: Tower of Ruin", 0x092);
            AddItemB("Ankh jewel: Chamber of Birth", 0x093);
            AddItemB("Ankh jewel: Twin Labyrinths", 0x094);
            AddItemB("Ankh jewel: Dimensional Corridor", 0x095);

            AddItemB("Sacred orb: Gate of Guidance", 0x0c7);
            AddItemB("Sacred orb: Surface", 0x0c8);
            AddItemB("Sacred orb: Mausoleum of the Giants", 0x0c9);
            AddItemB("Sacred orb: Temple of the Sun", 0x0ca);
            AddItemB("Sacred orb: Spring in the Sky", 0x0cb);
            AddItemB("Sacred orb: Chamber of Extinction", 0x0cc);
            AddItemB("Sacred orb: Twin Labyrinths", 0x0cd);
            AddItemB("Sacred orb: Shrine of the Mother", 0x0ce);
            AddItemB("Sacred orb: Tower of Ruin", 0x0cf);
            AddItemB("Sacred orb: Dimensional Corridor", 0x0d0);

            AddItemB("Map: Surface", 0xd1);
            AddItemB("Map: Gate of Guidance", 0xd2);
            AddItemB("Map: Mausoleum of the Giants", 0xd3);
            AddItemB("Map: Temple of the Sun", 0xd4);
            AddItemB("Map: Spring in the Sky", 0xd5);
            AddItemB("Map: Inferno Cavern", 0xd6);
            AddItemB("Map: Chamber of Extinction", 0xd7);
            AddItemB("Map: Twin Labyrinths", 0xd8);
            AddItemB("Map: Endless Corridor", 0xd9);
            AddItemB("Map: Shrine of the Mother", 0xda);
            AddItemB("Map: Gate of Illusion", 0xdb);
            AddItemB("Map: Graveyard of the Giants", 0xdc);
            AddItemB("Map: Temple of the Moon", 0xdd);
            AddItemB("Map: Tower of the Goddess", 0xde);
            AddItemB("Map: Tower of Ruin", 0xdf);
            AddItemB("Map: Chamber of Birth", 0xe0);
            AddItemB("Map: Dimensional Corridor", 0xe1);

            // Weapons
            AddItemW("Chain whip", 0x01);
            AddItemW("Flail whip", 0x02);
            AddItemW("Knife", 0x03);
            AddItemW("Key sword", 0x04);
            AddItemW("Axe", 0x05);
            AddItemW("Katana", 0x06);
            AddItemW("Key sword (unsealed)", 0x07);

            // Subweapons
            AddItemW("Shurikens", 0x08);
            AddItemW("Rolling shurikens", 0x09);
            AddItemW("Spears", 0x0a);
            AddItemW("Flares", 0x0b);
            AddItemW("Bombs", 0x0c);
            AddItemW("Chakrams", 0x0d);
            AddItemW("Caltrops", 0x0e);
            AddItemW("Pistol", 0x0f);
            AddItemW("Buckler", 0x10);
            // why anyone would want to split on this I don't know
            // also the value is set to 0x4b at some point, probably when getting the angel shield
            // if you hack out other shields, you actually have 75 fake silver shields and can break them one by one
            AddItemW("Fake silver shield", 0x4b);
            AddItemW("Silver shield", 0x11);
            AddItemW("Angel shield", 0x12);
            // 0x13 is ankh jewels, but it's not useful for splitting because it is just the ammo count

            // Usable items
            AddItemW("Hand scanner", 0x14);
            AddItemW("Djed pillar", 0x15);
            AddItemW("Mini doll", 0x16);
            AddItemW("Magatama jewel", 0x17);
            AddItemW("Cog of the soul", 0x18);
            AddItemW("Lamp (lit)", 0x019);
            AddItemW("Lamp of time", 0x50);
            AddItemW("Pochette key", 0x1a);
            AddItemW("Dragon bone", 0x1b, 0x9d); // probably a programming error because of the byte used for the item
            AddItemW("Crystal skull", 0x1c);
            AddItemW("Vessel", 0x1d);
            AddItemW("Medicine of the mind", 0x4d);
            AddItemW("Medicine of the mind (green)", 0x4e);
            AddItemW("Medicine of the mind (red)", 0x4f);
            AddItemW("Pepper", 0x1e);
            AddItemW("Woman statue", 0x1f);
            AddItemW("Maternity statue", 0x51);
            AddItemW("Key of eternity", 0x20);
            AddItemW("Serpent staff", 0x21);
            AddItemW("Talisman", 0x22);
            AddItemW("Diary", 0x48);
            AddItemW("La-Mulana Talisman", 0x49);
            // 0x23???

            // Treasures
            AddItemW("MSX2", 0x4c);
            AddItemW("Waterproof case", 0x24);
            AddItemW("Heatproof case", 0x25);
            AddItemW("Shellhorn", 0x26);
            AddItemW("Glove", 0x27);
            AddItemW("Holy grail", 0x28); // before filling with all memories of the ruins
            AddItemW("Holy grail (broken)", 0x52);
            AddItemW("Holy grail (filled)", 0x53);
            AddItemW("Isis' pendant", 0x29);
            AddItemW("Crucifix", 0x2a);
            AddItemW("Helmet", 0x2b);
            AddItemW("Grapple claw", 0x2c);
            AddItemW("Bronze mirror", 0x2d);
            AddItemW("Eye of truth", 0x2e);
            AddItemW("Ring", 0x2f);
            AddItemW("Scalesphere", 0x30);
            AddItemW("Gauntlet", 0x31);
            AddItemW("Treasures", 0x47);
            AddItemW("Anchor", 0x32);
            AddItemW("Plane model", 0x33);
            AddItemW("Ocarina", 0x34);
            AddItemW("Feather", 0x35);
            AddItemW("Book of the dead", 0x36);
            AddItemW("Fairy clothes", 0x37);
            AddItemW("Scriptures", 0x38);
            AddItemW("Hermes' boots", 0x39);
            AddItemW("Fruit of Eden", 0x3a);
            AddItemW("Twin statue", 0x3b);
            AddItemW("Bracelet", 0x3c);
            AddItemW("Perfume", 0x3d);
            AddItemW("Spaulder", 0x3e);
            AddItemW("Dimensional key", 0x3f);
            AddItemW("Ice cape", 0x40);

            AddItemW("Origin seal", 0x41);
            AddItemW("Birth seal", 0x42);
            AddItemW("Life seal", 0x43);
            AddItemW("Death seal", 0x44);

            AddItemW("Forbidden treasure", 0x4a);

            AddItemW("reader.exe", 0x55);
            AddItemW("xmailer.exe", 0x56);
            AddItemW("yagomap.exe", 0x57);
            AddItemW("yagostr.exe", 0x58);
            AddItemW("bunemon.exe", 0x59);
            AddItemW("buneplus.com", 0x5a);
            AddItemW("torude.exe", 0x5b);
            AddItemW("guild.exe", 0x5c);
            AddItemW("mantra.exe", 0x5d);
            AddItemW("emusic.exe", 0x5e);
            AddItemW("beolamu.exe", 0x5f);
            AddItemW("deathv.exe", 0x60);
            AddItemW("randc.exe", 0x61);
            AddItemW("capstar.exe", 0x62);
            AddItemW("move.exe", 0x63);
            AddItemW("mekuri.exe", 0x64);
            AddItemW("bounce.exe", 0x65);
            AddItemW("miracle.exe", 0x66);
            AddItemW("mirai.exe", 0x67);
            AddItemW("lamulana.exe", 0x68);

            AddEvent("Score=12", 0x07b, 12);
            AddEvent("Luck fairy", () => (0x200 == (0x200 & game.Var<uint>("flags-2"))));
            AddEvent("Fairies on cooldown", () => (0x10 == (0x10 & game.Var<uint>("flags-2"))));
            AddEvent("Fairies off cooldown", () => (0 == (0x10 & game.Var<uint>("flags-2"))));

            AddEvent("Guidance jewel puzzle step 1", 0x136, 1);
            AddEvent("Mausoleum jewel chest", 0x163);
            AddEvent("Guidance jewel puzzle step 2", 0x136, 2);
            AddEvent("Guidance jewel puzzle", 0x136, 3);
            AddEvent("Amphisbaena ankh puzzle step 1", 0x136, 2);
            AddEvent("Amphisbaena ankh puzzle", 0x136, 5);

            AddEvent("Sakit ankh", 0x164);

            AddEvent("Buer", 0x17a);
            AddEvent("Jump into the sun puzzle", 0x17e, 2);

            AddEvent("Nuckelavee", 0x191);
            AddEvent("Left hatch", 0x194);
            AddEvent("Right hatch", 0x195);
            AddEvent("Floodgates", 0x199);

            AddEvent("Peryton", 0x1d8);
            AddEvent("Black witch", 0x1df);

            AddEvent("Visit fairy queen", 0x1f5, 1);
            AddEvent("Fairies", 0x1f5, 2);

            AddEvent("Eden", 0x226);
            AddEvent("Read cogs tablets", () => (getbyte(0x23b) >= 1 && getbyte(0x23c) >= 1 && getbyte(0x23d) >= 1));

            AddEvent("Grave lift activated", 0x249);

            AddEvent("Grind down the watchtower step 1", 0x266, 1);
            AddEvent("Grind down the watchtower", 0x266, 2); // fixme: might include the softlock state
            AddEvent("Jumped into the sun", 0x267, 1);
            AddEvent("Eden spot #2", 0x270);
            AddEvent("Lit the tower of the goddess", 0x271);
            AddEvent("Eden spot #1", 0x29e);
            AddEvent("Anubis visit", 0x32a);

            AddEvent("Coin chest: Mausoleum", 0x166);

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
