cpubasetype data=popi();
cpubasetype addr=popi();
if (addr>=memory.FloatOffset) //set int as float
{
    
    memory.floatdata[addr-memory.FloatOffset]=*(double *)(&data);
}
else
{
    memory.intdata[addr]=data;
}