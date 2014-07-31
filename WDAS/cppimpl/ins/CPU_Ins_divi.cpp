cpubasetype a, b;
a = popi();
b = popi();
if (a==0) //a==0
{
    pushi(0);
    pushi(0);
    Interrupt(0); //div
}
else
{
    pushi(b%a);
    pushi(b/a);
}