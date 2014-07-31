double data=popf();
cpubasetype addr=popi();
if (addr<memory.FloatOffset) //set float as int
{
    
    memory.intdata[addr]=*(cpubasetype *)(&data);
}
else
{
    memory.floatdata[addr-memory.FloatOffset]=data;
}