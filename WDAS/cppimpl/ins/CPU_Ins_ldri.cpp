cpubasetype addr=popi();
if (addr>=memory.FloatOffset) //load float as int
{
    pushi(*(cpubasetype *)(&memory.floatdata[addr-memory.FloatOffset]));
}
else
{
    pushi(memory.intdata[addr]);
}