cpubasetype addr=popi();
if (addr<memory.FloatOffset) //load int as float
{
    pushf(*(double *)(&memory.intdata[addr]));
}
else
{
    pushf(memory.floatdata[addr-memory.FloatOffset]);
}