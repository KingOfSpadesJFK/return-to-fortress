using Godot;
using System;

[GlobalClass]
public partial class ImpactDamage : ImpactResource
{
    [Export] public int Damage = 10;
    
    public override void Impact()
    {
        GD.Print("ImpactDamage.Impact()");
    }
}
