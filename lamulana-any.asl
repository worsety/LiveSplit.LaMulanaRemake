state("LaMulanaWin")
{
	uint igt : 0x2D724C;
	uint flags1 : 0x2D7BC8;
	uint flags2 : 0x2D7BCC;
	uint flags3 : 0x2D7BD0;
	uint flags4 : 0x2D7BD4;
	uint flags5 : 0x2D7BD8;
}

isLoading
{
	return true;
}

gameTime
{
	return TimeSpan.FromMilliseconds(current.igt);
}

init
{

	Func<int,byte> getbyte = (x) => (memory.ReadValue<byte>(modules.First().BaseAddress + 0x2D7E80 + x));
	Func<int,short> getword = (x) => (new DeepPointer(0x2D7858, x * 2).Deref<short>(game));
	Func<bool> inshop = () => (0x800000 == (current.flags4 & 0x800000));
	vars.splits = new Func<bool>[] {
		//() => (getbyte(0x07b) == 12), // talked to xelpud twice
		//() => (getbyte(0x14f) == 2), // deathv?
		//() => (getbyte(0x147) == 1), // ruins shortcut
		//() => (getbyte(0x0c7) == 2), // sacred orb: guidance
		//() => (getbyte(0x0a9) == 2), // grail
		() => (getbyte(0x0ba) == 2), // hermes boots
		() => (getword(0x01b) == 0x9d && !inshop()), // dragon bone, why 9d!??? It's never replaced
		() => (getbyte(0x0f8) == 3), // ellmac
		() => (getbyte(0x0f9) == 3), // bahamut
		() => (getbyte(0x0f7) == 3), // sakit
		() => (getbyte(0x0fc) == 3), // baphomet
		() => (getbyte(0x0fa) == 3), // viy
		() => (getbyte(0x0f6) == 3), // amphisbaena
		() => (getbyte(0x0fd) == 3), // tiamat
		() => (getbyte(0x0fb) == 3), // palenque
		() => (getbyte(0x0fe) == 3), // mother
		() => (0x8020001 == (0x8020001 & current.flags4)), // escape
	};
}

start
{
	if (0x10000 == (0x10000 & current.flags5)) {
		vars.cursplit = 0;
		return true;
	}
	return false;
}

split
{
	if (vars.cursplit >= vars.splits.Length)
		return false;
	if (vars.splits[vars.cursplit]()) {
		print(vars.cursplit.ToString());
		vars.cursplit++;
		return true;
	}
	return false;
}
