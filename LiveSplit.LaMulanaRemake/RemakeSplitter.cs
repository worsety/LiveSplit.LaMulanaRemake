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

        public RemakeSplitter() : base()
        {
            game = new EyeOfTruth.LaMulanaRemake();

            Func<int, Func<bool>> bytenz = (idx) => () => (getbyte(idx) != 0);
            Func<int, byte, Func<bool>> byteeq = (idx, val) => () => (getbyte(idx) == val);
            Func<int, byte, Func<bool>> bytege = (idx, val) => () => (getbyte(idx) >= val);
            Func<int, Func<bool>> wordnz = (idx) => () => (getword(idx) != 0);
            Func<int, ushort, Func<bool>> wordeq = (idx, val) => () => (getword(idx) == val);
            Func<int, ushort, Func<bool>> wordge = (idx, val) => () => (getword(idx) >= val);

            AddCond(() => (0x8020001 == (0x8020001 & game.Var<uint>("flags-4"))), "Escape");

            Action<int, string> AddGuardian = (idx, guardian) =>
            {
                AddCond(bytege(idx, 2), guardian + " start");
                AddCond(bytege(idx, 3), guardian);
            };
            StartCat("Guardians");
            AddGuardian(0x0f6, "Amphisbaena");
            AddGuardian(0x0f7, "Sakit");
            AddGuardian(0x0f8, "Ellmac");
            AddGuardian(0x0f9, "Bahamut");
            AddGuardian(0x0fa, "Viy");
            AddGuardian(0x0fb, "Baphomet");
            AddGuardian(0x0fc, "Palenque");
            AddGuardian(0x0fd, "Tiamat");
            AddGuardian(0x0fe, "Mother");
            EndCat();

            Action<int, string> AddGrail = (idx, field) => AddCond(byteeq(idx, 1), "Grail point: " + field, field);
            StartCat("Grail points");
            AddGrail(0x064, "Gate of Guidance");
            AddGrail(0x065, "Mausoleum of the Giants");
            AddGrail(0x066, "Temple of the Sun");
            AddGrail(0x067, "Spring in the Sky");
            AddGrail(0x068, "Inferno Cavern");
            AddGrail(0x069, "Chamber of Extinction");
            AddGrail(0x06a, "Twin Labyrinths (front)");
            AddGrail(0x06b, "Endless Corridor");
            AddGrail(0x06d, "Gate of Illusion");
            AddGrail(0x06e, "Graveyard of the Giants");
            AddGrail(0x06f, "Temple of the Moon");
            AddGrail(0x070, "Tower of the Goddess");
            AddGrail(0x071, "Tower of Ruin");
            AddGrail(0x072, "Chamber of Birth");
            AddGrail(0x073, "Twin Labyrinths (back)");
            AddGrail(0x074, "Dimensional Corridor");
            AddGrail(0x06c, "Shrine of the Mother");
            AddGrail(0x075, "True Shrine of the Mother");
            EndCat();

            Action<int, string> AddJewel = (idx, field) => AddCond(byteeq(idx, 2), "Ankh jewel: " + field, field);
            StartCat("Ankh jewels");
            AddJewel(0x08e, "Gate of Guidance");
            AddJewel(0x08f, "Mausoleum of the Giants");
            AddJewel(0x090, "Temple of the Sun");
            AddJewel(0x091, "Spring in the Sky");
            AddJewel(0x092, "Tower of Ruin");
            AddJewel(0x093, "Chamber of Birth");
            AddJewel(0x094, "Twin Labyrinths");
            AddJewel(0x095, "Dimensional Corridor");
            EndCat();

            Action<int, string> AddOrb = (idx, field) => AddCond(byteeq(idx, 2), "Sacred orb: " + field, field);
            StartCat("Sacred orbs");
            AddOrb(0x0c8, "Surface");
            AddOrb(0x0c7, "Gate of Guidance");
            AddOrb(0x0c9, "Mausoleum of the Giants");
            AddOrb(0x0ca, "Temple of the Sun");
            AddOrb(0x0cb, "Spring in the Sky");
            AddOrb(0x0cc, "Chamber of Extinction");
            AddOrb(0x0cd, "Twin Labyrinths");
            AddOrb(0x0cf, "Tower of Ruin");
            AddOrb(0x0d0, "Dimensional Corridor");
            AddOrb(0x0ce, "Shrine of the Mother");
            EndCat();

            Action<int, string> AddMap = (idx, field) => AddCond(byteeq(idx, 2), "Map: " + field, field);
            StartCat("Maps");
            AddMap(0xd1, "Surface");
            AddMap(0xd2, "Gate of Guidance");
            AddMap(0xd3, "Mausoleum of the Giants");
            AddMap(0xd4, "Temple of the Sun");
            AddMap(0xd5, "Spring in the Sky");
            AddMap(0xd6, "Inferno Cavern");
            AddMap(0xd7, "Chamber of Extinction");
            AddMap(0xd8, "Twin Labyrinths");
            AddMap(0xd9, "Endless Corridor");
            AddMap(0xdb, "Gate of Illusion");
            AddMap(0xdc, "Graveyard of the Giants");
            AddMap(0xdd, "Temple of the Moon");
            AddMap(0xde, "Tower of the Goddess");
            AddMap(0xdf, "Tower of Ruin");
            AddMap(0xe0, "Chamber of Birth");
            AddMap(0xe1, "Dimensional Corridor");
            AddMap(0xda, "Shrine of the Mother");
            EndCat();

            Action<int, string> AddItemB = (idx, name) => AddCond(() => getbyte(idx) == 2 && !inshop(), name);
            Action<int, string> AddItemW = (idx, name) => AddCond(() => getword(idx) != 0 && !inshop(), name);
            StartCat("Weapons");
            AddItemW(0x01, "Chain whip");
            AddItemW(0x02, "Flail whip");
            AddItemW(0x03, "Knife");
            AddItemW(0x04, "Key sword");
            AddItemW(0x05, "Axe");
            AddItemW(0x06, "Katana");
            AddItemW(0x07, "Key sword (unsealed)");
            EndCat();

            StartCat("Subweapons");
            AddItemW(0x08, "Shurikens");
            AddItemW(0x09, "Rolling shurikens");
            AddItemW(0x0a, "Spears");
            AddItemW(0x0b, "Flares");
            AddItemW(0x0c, "Bombs");
            AddItemW(0x0d, "Chakrams");
            AddItemW(0x0e, "Caltrops");
            AddItemW(0x0f, "Pistol");
            AddItemW(0x10, "Buckler");
            // why anyone would want to split on this I don't know
            // also the value is set to 0x4b at some point, probably when getting the angel shield
            // if you hack out other shields, you actually have 75 fake silver shields and can break them one by one
            AddItemW(0x4b, "Fake silver shield");
            AddItemW(0x11, "Silver shield");
            AddItemW(0x12, "Angel shield");
            // 0x13 is ankh jewels, but it's not useful for splitting because it is just the ammo count
            EndCat();

            StartCat("Usable items");
            AddItemW(0x14, "Hand scanner");
            AddItemW(0x15, "Djed pillar");
            AddItemW(0x16, "Mini doll");
            AddItemW(0x17, "Magatama jewel");
            AddItemW(0x18, "Cog of the soul");
            AddItemW(0x019, "Lamp (lit)");
            AddItemW(0x50, "Lamp of time");
            AddItemW(0x1a, "Pochette key");
            AddItemW(0x1b, "Dragon bone"); // probably a programming error because of the byte used for the item
            AddItemW(0x1c, "Crystal skull");
            AddItemW(0x1d, "Vessel");
            AddItemW(0x4d, "Medicine of the mind");
            AddItemW(0x4e, "Medicine of the mind (green)");
            AddItemW(0x4f, "Medicine of the mind (red)");
            AddItemW(0x1e, "Pepper");
            AddItemW(0x1f, "Woman statue");
            AddItemW(0x51, "Maternity statue");
            AddItemW(0x20, "Key of eternity");
            AddItemW(0x21, "Serpent staff");
            AddItemW(0x22, "Talisman");
            AddItemW(0x48, "Diary");
            AddItemW(0x49, "La-Mulana Talisman");
            // 0x23???
            EndCat();

            StartCat("Treasures");
            AddItemW(0x4c, "MSX2");
            AddItemW(0x24, "Waterproof case");
            AddItemW(0x25, "Heatproof case");
            AddItemW(0x26, "Shellhorn");
            AddItemW(0x27, "Glove");
            AddItemW(0x28, "Holy grail"); // before filling with all memories of the ruins
            AddItemW(0x52, "Holy grail (broken)");
            AddItemW(0x53, "Holy grail (filled)");
            AddItemW(0x29, "Isis' pendant");
            AddItemW(0x2a, "Crucifix");
            AddItemW(0x2b, "Helmet");
            AddItemW(0x2c, "Grapple claw");
            AddItemW(0x2d, "Bronze mirror");
            AddItemW(0x2e, "Eye of truth");
            AddItemW(0x2f, "Ring");
            AddItemW(0x30, "Scalesphere");
            AddItemW(0x31, "Gauntlet");
            AddItemW(0x47, "Treasures");
            AddItemW(0x32, "Anchor");
            AddItemW(0x33, "Plane model");
            AddItemW(0x34, "Ocarina");
            AddItemW(0x35, "Feather");
            AddItemW(0x36, "Book of the dead");
            AddItemW(0x37, "Fairy clothes");
            AddItemW(0x38, "Scriptures");
            AddItemW(0x39, "Hermes' boots");
            AddItemW(0x3a, "Fruit of Eden");
            AddItemW(0x3b, "Twin statue");
            AddItemW(0x3c, "Bracelet");
            AddItemW(0x3d, "Perfume");
            AddItemW(0x3e, "Spaulder");
            AddItemW(0x3f, "Dimensional key");
            AddItemW(0x40, "Ice cape");
            EndCat();

            StartCat("Seals");
            AddItemW(0x41, "Origin seal");
            AddItemW(0x42, "Birth seal");
            AddItemW(0x43, "Life seal");
            AddItemW(0x44, "Death seal");
            AddItemW(0x4a, "Forbidden treasure");
            EndCat();

            StartCat("Software");
            AddItemW(0x55, "reader.exe");
            AddItemW(0x56, "xmailer.exe");
            AddItemW(0x57, "yagomap.exe");
            AddItemW(0x58, "yagostr.exe");
            AddItemW(0x59, "bunemon.exe");
            AddItemW(0x5a, "buneplus.com");
            AddItemW(0x5b, "torude.exe");
            AddItemW(0x5c, "guild.exe");
            AddItemW(0x5d, "mantra.exe");
            AddItemW(0x5e, "emusic.exe");
            AddItemW(0x5f, "beolamu.exe");
            AddItemW(0x60, "deathv.exe");
            AddItemW(0x61, "randc.exe");
            AddItemW(0x62, "capstar.exe");
            AddItemW(0x63, "move.exe");
            AddItemW(0x64, "mekuri.exe");
            AddItemW(0x65, "bounce.exe");
            AddItemW(0x66, "miracle.exe");
            AddItemW(0x67, "mirai.exe");
            AddItemW(0x68, "lamulana.exe");
            EndCat();

            StartCat("Puzzles");
            Action<int, string> AddMantra = (idx, name) =>
            {
                AddCond(bytege(idx, 3), String.Format("Learnt {0}", name));
                AddCond(bytege(idx, 4), String.Format("Recited {0}", name));
            };
            StartCat("Mantras");
            AddMantra(0x12b, "MARDUK");
            AddMantra(0x12a, "SABBAT");
            AddMantra(0x129, "MU");
            AddMantra(0x128, "VIY");
            AddMantra(0x127, "BAHRUN");
            AddMantra(0x126, "WEDJET");
            AddMantra(0x125, "ABUTO");
            AddMantra(0x124, "LAMULANA");
            EndCat();

            StartCat("Surface");
            AddCond(bytege(0x07b, 12), "Score=12", "Talk to Xelpud twice");
            AddCond(bytege(0x145, 1), "Ruins open", "Ruins open");
            AddCond(bytege(0x148, 1), "Ruins shortcut", "Ruins shortcut");
            AddCond(bytege(0x148, 1), "Argus");
            AddCond(bytege(0x14c, 1), "Surface hidden path", "Revealed hidden path"); // probably set at a funky time in any%, like when going to oxhead and horseface
            EndCat();

            StartCat("Gate of Guidance");
            AddCond(bytege(0x133, 1), "Amphisbaena ankh 1", "Ankh: block");
            AddCond(bytege(0x133, 2), "Amphisbaena ankh 2", "Ankh: left dais");
            AddCond(bytege(0x133, 5), "Amphisbaena ankh 5", "Ankh: right dais");
            AddCond(bytege(0x136, 1), "Guidance jewel 1", "Jewel: break blocks");
            AddCond(bytege(0x136, 2), "Guidance jewel 2", "Jewel: 1st dais");
            AddCond(bytege(0x136, 3), "Guidance jewel 3", "Jewel: 2nd dais");
            AddCond(bytege(0x134, 1), "Guidance elevator", "Elevator");
            AddCond(bytege(0x143, 1), "Guidance G-2 coin chests dais", "G-2 coin chests dais");
            EndCat();

            StartCat("Mausoleum of the Giants");
            AddCond(bytege(0x164, 1), "Sakit ankh", "Ankh");
            AddCond(bytege(0x163, 1), "Mausoleum jewel puz", "Ankh jewel dais");
            AddCond(bytege(0x165, 1), "Mausoleum orb puz", "Futo dais");
            AddCond(byteeq(0x15f, 1), "Skydisk: Star");
            AddCond(byteeq(0x15f, 2), "Skydisk: Moon");
            AddCond(byteeq(0x15f, 3), "Skydisk: Sun");
            AddCond(bytege(0x16a, 2), "Hard mode");
            EndCat();

            StartCat("Temple of the Sun");
            AddCond(bytege(0x178, 1), "Sun ankh 1", "Ankh: unlock minecart");
            AddCond(bytege(0x178, 5), "Sun ankh 5", "Ankh: minecart at ellmac");
            AddCond(bytege(0x18f, 1), "Sun jewel puz", "Meditate under Wedjet");
            AddCond(bytege(0x17a, 1), "Buer");
            AddCond(bytege(0x267, 1), "Jumped into the sun"); // actually a temple of the moonlight flag for the moon opening animation
            AddCond(bytenz(0x391), "Mulbruk"); // there are other flags we could check but this is the one set on the frame where your score is increased to 50, relevant to some routes
            EndCat();

            StartCat("Spring in the Sky");
            AddCond(bytege(0x19f, 1), "Spring ankh", "Ankh");
            AddCond(bytege(0x191, 1), "Nuckelavee");
            AddCond(bytege(0x193, 1), "Floodgates");
            AddCond(bytege(0x199, 1), "Aqueduct");
            AddCond(bytege(0x194, 1), "Left hatch");
            AddCond(bytege(0x195, 1), "Right hatch");
            AddCond(bytege(0x078, 1), "Giltoriyo");
            AddCond(bytege(0x387, 1), "Fishy merchandise");
            EndCat();

            StartCat("Inferno Cavern");
            AddCond(bytege(0x1b4, 1), "Inferno ankh 1", "Ankh: triggered dais rising");
            AddCond(bytege(0x1b4, 4), "Inferno ankh 4", "Ankh: placed weight");
            AddCond(bytege(0x1b3, 1), "Flare puzzle wall");
            EndCat();

            StartCat("Chamber of Extinction");
            AddCond(bytege(0x1c3, 1), "Extinction ankh 1", "Palenque ankh step 1");
            AddCond(bytege(0x1c3, 3), "Extinction ankh 3", "Palenque ankh step 2");
            AddCond(bytege(0x1c6, 1), "Centimani");
            AddCond(bytege(0x1cb, 1), "Ox-head and Horse-face");
            AddCond(bytege(0x1c7, 1), "Lit mantra.exe mural");
            AddCond(bytege(0x38c, 1), "Gate of Time");
            EndCat();

            StartCat("Twin Labyrinths");
            AddCond(bytege(0x1d8, 1), "Peryton");
            AddCond(bytege(0x1e2, 1), "Zu");
            AddCond(bytege(0x1df, 1), "Black witch");
            AddCond(bytege(0x1e0, 1), "White witch");
            AddCond(bytege(0x1e5, 1), "Reveal light (left)");
            AddCond(bytege(0x1de, 1), "Reveal light (right)");
            AddCond(bytege(0x1dc, 2), "Release the twins");
            EndCat();

            StartCat("Endless Corridor");
            AddCond(bytege(0x1f5, 1), "Visit fairy queen");
            AddCond(bytenz(0x118), "Fairies"); // this is the actual fairy points condition, 1f5 is the fairy queen's dialogue for the first two visits
            AddCond(bytege(0x1f8, 2), "1st corridor");
            AddCond(bytege(0x1f9, 2), "2nd corridor");
            AddCond(bytege(0x1fa, 2), "3rd corridor");
            AddCond(bytege(0x1fb, 2), "4th corridor");
            AddCond(bytege(0x1fc, 1), "5th corridor");
            EndCat();

            StartCat("Gate of Illusion");
            AddCond(bytege(0x226, 1), "Eden", "Dispel Eden illusion");
            AddCond(bytege(0x236, 1), "Crush the hand");
            AddCond(() => (getbyte(0x23b) >= 1 && getbyte(0x23c) >= 1 && getbyte(0x23d) >= 1), "Read cog tablets", "Read cog riddles");
            AddCond(bytege(0x23b, 2), "Ba");
            AddCond(bytege(0x23c, 3), "Lizard");
            AddCond(bytege(0x23d, 2), "Child riddle", "Child");
            AddCond(bytege(0x23a, 1), "Cog 1", "Stray fairy");
            AddCond(bytege(0x23a, 4), "Cog 4", "Use cog");
            AddCond(bytege(0x239, 1), "Close sacrificial pit");
            AddCond(bytege(0x237, 1), "Chi You");
            EndCat();

            StartCat("Graveyard of the Giants");
            AddCond(bytege(0x249, 1), "Grave lift activated", "Lift activated");
            AddCond(bytege(0x24a, 1), "Gauntlet barrier");
            EndCat();

            StartCat("Temple of Moonlight");
            AddCond(bytege(0x266, 1), "Grind down the watchtower 1");
            AddCond(bytege(0x266, 2), "Grind down the watchtower 2"); // fixme: might include the softlock state
            AddCond(bytege(0x270, 1), "Eden: dancing man");
            AddCond(bytege(0x29c, 1), "Eden: hands");
            AddCond(bytege(0x29d, 1), "Eden: trap");
            AddCond(bytege(0x29e, 1), "Eden: face");
            AddCond(bytege(0x32a, 1), "Anubis visit"); // technically this is a Mulbruk conversation flag
            AddCond(bytege(0x07a, 1), "Alsedana");
            EndCat();

            StartCat("Tower of the Goddess");
            AddCond(bytege(0x271, 1), "Lit the tower of the goddess", "Lights on");
            AddCond(bytege(0x275, 2), "Eye of Truth 2", "Eye of Truth");
            AddCond(bytege(0x41c, 1), "Unlock hatches");
            AddCond(bytege(0x27b, 1), "Flail 1", "Start Flail Whip puzzle");
            AddCond(bytege(0x27f, 1), "Vimana (left)");
            AddCond(bytege(0x280, 1), "Vimana (right)");
            AddCond(bytege(0x38e, 1), "Reveal Spaulder"); // maybe ACE unlocks this while going somewhere but doesn't pick it up until mother
            EndCat();

            StartCat("Tower of Ruin");
            AddCond(bytege(0x28e, 1), "Thunderbird");
            AddCond(bytege(0x28f, 1), "Uncover medicine statue");
            AddCond(bytege(0x298, 1), "Djed pillar 1");
            AddCond(bytege(0x298, 3), "Djed pillar 2");
            AddCond(bytege(0x298, 3), "Nuwa");
            AddCond(bytege(0x529, 1), "Ruin La-Mulanese", "La-Mulanese tablet");
            EndCat();

            StartCat("Chamber of Birth");
            AddCond(bytege(0x531, 1), "Birth La-Mulanese", "La-Mulanese tablet");
            AddCond(bytege(0x2a6, 1), "Dance of life");
            AddCond(bytege(0x2a6, 2), "Skanda");
            EndCat();

            StartCat("Dimensional Corridor");
            AddCond(bytege(0x10d, 1), "Fobos");
            AddCond(() => (getbyte(0x545) != 0 && getbyte(0x546) != 0), "Learn BIRTH + DEATH");
            AddCond(bytege(0x2c3, 1), "Girtablilu");
            AddCond(bytege(0x2c7, 1), "Ugallu");
            AddCond(bytege(0x2c9, 1), "Lahamu");
            AddCond(bytege(0x2c8, 1), "Ushum");
            AddCond(bytege(0x2c5, 1), "Kuusarikku");
            AddCond(bytege(0x2c4, 1), "Kulullu");
            AddCond(bytege(0x2c6, 1), "Urmahlullu");
            AddCond(bytege(0x2ca, 1), "Mushnahhu");
            AddCond(bytege(0x2cc, 1), "Ushumgallu");
            AddCond(bytege(0x2cb, 1), "Umu-dabrutu");
            AddCond(bytege(0x2d4, 1), "Mushussu");
            AddCond(bytege(0x2c0, 1), "Mushussu puzzle 1");
            AddCond(bytege(0x2c0, 2), "Mushussu puzzle 2");
            AddCond(bytege(0x2c0, 3), "Mushussu puzzle 3");
            AddCond(bytege(0x2c0, 4), "Mushussu puzzle 4");
            EndCat();

            StartCat("Shrine of the Mother");
            AddCond(bytege(0x218, 1), "Placed Dragon Bone");
            AddCond(bytege(0x4e2, 1), "Shrine La-Mulanese", "La-Mulanese tablet");
            EndCat();

            StartCat("True Shrine of the Mother");
            AddCond(bytege(0x2e0, 1), "Mom ankh 1", "Ankh dais");
            AddCond(bytege(0x2d5, 2), "Asked Fairies for help");
            EndCat();

            StartCat("Hell Temple");
            AddCond(bytege(0x3bb, 1), "Unlock Hell Temple");
            AddCond(bytege(0x106, 2), "Beat Hell Temple");
            EndCat();
            EndCat();

            Action<int, string> AddCoin = (idx, name) => AddCond(bytenz(idx), "Coin chest: " + name, name);
            StartCat("Coin chests");
            AddCoin(0x14d, "Surface D-3");
            AddCoin(0x155, "Surface J-2");
            AddCoin(0x156, "Surface K-1");
            AddCoin(0x138, "Gate of Guidance G-2 top");
            AddCoin(0x13b, "Gate of Guidance G-2 bot");
            AddCoin(0x13c, "Gate of Guidance C-4");
            AddCoin(0x166, "Mausoleum of the Giants");
            AddCoin(0x18b, "Temple of the Sun");
            AddCoin(0x1a2, "Spring in the Sky");
            AddCoin(0x1ab, "Inferno Cavern E-2");
            AddCoin(0x1ba, "Inferno Cavern C-1");
            AddCoin(0x401, "Chamber of Extinction");
            AddCoin(0x1ec, "Twin Labyrinths B-4");
            AddCoin(0x1ee, "Twin Labyrinths H-3");
            AddCoin(0x22c, "Gate of Illusion F-6");
            AddCoin(0x233, "Gate of Illusion A-4");
            AddCoin(0x242, "Graveyard of the Giants");
            AddCoin(0x25a, "Temple of Moonlight");
            AddCoin(0x27e, "Tower of the Goddess B-5");
            AddCoin(0x286, "Tower of the Goddess E-1");
            AddCoin(0x2bb, "Tower of Ruin");
            AddCoin(0x2aa, "Chamber of Birth E-2");
            AddCoin(0x2b1, "Chamber of Birth H-4");
            AddCoin(0x2b3, "Chamber of Birth E-6");
            AddCoin(0x2bf, "Dimensional Corridor");
            AddCoin(0x216, "Shrine of the Mother");
            AddCoin(0x3fc, "Escape");
            EndCat();

            StartCat("Fairy points");
            AddCond(bytenz(0x392), "Fairy point: Spring in the Sky", "Spring in the Sky");
            AddCond(bytenz(0x393), "Fairy point: Chamber of Extinction", "Chamber of Extinction");
            AddCond(bytenz(0x398), "Fairy point: Twin Labyrinths", "Twin Labyrinths");
            AddCond(bytenz(0x395), "Fairy point: Endless Corridor", "Endless Corridor");
            AddCond(bytenz(0x399), "Fairy point: Gate of Illusion", "Gate of Illusion");
            AddCond(bytenz(0x396), "Fairy point: Temple of Moonlight", "Temple of Moonlight");
            AddCond(bytenz(0x397), "Fairy point: Tower of the Goddess", "Tower of the Goddess");
            AddCond(bytenz(0x394), "Fairy point: Shrine of the Mother", "Shrine of the Mother");
            AddCond(bytenz(0x39a), "Fairy point: True Shrine of the Mother", "True Shrine of the Mother");
            EndCat();

            Action<int, int> AddEmail = (idx, num) => AddCond(bytenz(idx), String.Format("Xelpud's e-mail #{0:D2}", num), String.Format("#{0:D2}", num));
            StartCat("E-Mails");
            AddEmail(0x2ee, 0);
            AddEmail(0x2ef, 1);
            AddEmail(0x349, 2);
            AddEmail(0x36e, 3);
            for (int i = 0; i < 41; i++)
                AddEmail(0x2f0 + i, i + 4);
            EndCat();

            StartCat("Misc");
            AddCond(() => (0x200 == (0x200 & game.Var<uint>("flags-2"))), "Luck fairy");
            AddCond(() => (0x10 == (0x10 & game.Var<uint>("flags-2"))), "Fairies on cooldown");
            AddCond(() => (0 == (0x10 & game.Var<uint>("flags-2"))), "Fairies off cooldown");
            AddCond(bytege(0x2e5, 1), "20% La-Mulanese");
            AddCond(bytege(0x2e5, 2), "60% La-Mulanese");
            AddCond(bytege(0x2e5, 3), "100% La-Mulanese");
            AddCond(wordnz(0x19), "Light lamp");
            EndCat();

            foreach (var key in intsplits.Keys)
                splits[key.Normalize().ToLowerInvariant()] = key;
        }
    }
}
