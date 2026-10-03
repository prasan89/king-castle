# ScriptableObject Assets to Create in Unity Editor

## How to create:
Right-click in Project window → Create → KingSmash/Config/...

## Required for M1_TestLevel:

### MaterialConfig assets (create 3):
Assets/ScriptableObjects/Config/Wood_Material.asset
  materialType: Wood
  displayName: Wood
  maxHealth: 80
  minDamageSpeed: 1.5
  damageMultiplier: 1.2
  density: 0.6
  bounciness: 0.2
  friction: 0.5
  debrisCount: 4
  breakParticleScale: 0.8
  impactSoundKey: impact_wood
  breakSoundKey: break_wood
  debugColor: #8B4513 (brown)

Assets/ScriptableObjects/Config/Stone_Material.asset
  materialType: Stone
  displayName: Stone
  maxHealth: 200
  minDamageSpeed: 2.5
  damageMultiplier: 0.7
  density: 2.5
  bounciness: 0.15
  friction: 0.6
  debrisCount: 3
  breakParticleScale: 1.0
  impactSoundKey: impact_stone
  breakSoundKey: break_stone
  debugColor: #808080 (grey)

Assets/ScriptableObjects/Config/Metal_Material.asset
  materialType: Metal
  displayName: Metal
  maxHealth: 400
  minDamageSpeed: 4.0
  damageMultiplier: 0.4
  density: 7.0
  bounciness: 0.5
  friction: 0.3
  debrisCount: 2
  breakParticleScale: 0.6
  impactSoundKey: impact_metal
  breakSoundKey: break_metal
  debugColor: #C0C0C0 (silver)

### ProjectileConfig asset:
Assets/ScriptableObjects/Config/KingProjectile_Config.asset
  minLaunchPower: 4
  maxLaunchPower: 22
  gravityScale: 1.5
  airDrag: 0.02
  maxBounces: 2
  bounceEnergyLoss: 0.4
  impactForce: 800
  minSpeedForDamage: 1.5
  trailTime: 0.35
  aimLineDots: 35
  aimLineDotSpacing: 0.18

### LevelConfig asset:
Assets/ScriptableObjects/Config/M1TestLevel_Config.asset
  levelIndex: 0
  worldIndex: 0
  displayName: Test Level
  kingLaunches: 5
  objective:
    requiredScore: 500
    twoStarScore: 1200
    threeStarScore: 2000
