using Godot;
using System;

public interface IDamagable
{
    public void Damage(int damage, Vector3 knockback = new Vector3());
}