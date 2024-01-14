using Godot;
using System;

[GlobalClass]
public partial class ProjectileInfo : Resource
{
    [Export] public Mesh ProjectileMesh;
    [Export] public Skin ProjectileSkin;
    [Export] public Mesh[] ParticleMesh;
    [Export] public Skin ParticleSkin;
    [Export] public float ParticleLifetime = 1.0f;
    [Export] public ParticleProcessMaterial ParticleMaterial;
    [Export] public ImpactResource Impaction;
    [Export] public ImpactResource TimeoutImpaction;
    [Export] public float TimeoutTimer = 10.0f;
    public PackedScene Node;

    public ProjectileInfo(Mesh projM, Skin projS, Mesh[] partM, Skin partS, ParticleProcessMaterial partMat, ImpactResource impact, ImpactResource timeImpact)
    {
        ProjectileMesh = projM;
        ProjectileSkin = projS;
        ParticleMesh = partM;
        ParticleSkin = partS;
        ParticleMaterial = partMat;
        Impaction = impact;
        TimeoutImpaction = timeImpact;
    }

    public ProjectileInfo() : this(null, null, null, null, null, null, null) {}
}
