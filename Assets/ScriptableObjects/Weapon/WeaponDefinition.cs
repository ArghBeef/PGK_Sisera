using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_", menuName = "Weapons/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Info")]
    public string weaponId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Combat")]
    [Min(0.01f)] public float fireRate = 5f;
    [Min(0f)] public float damage = 10f;
    [Min(1)] public int magazineSize = 12;
    [Min(0)] public int magazines = 3;
    [Min(0.05f)] public float reloadTime = 1.5f;
    [Min(0.1f)] public float range = 20f;
    [Min(1)] public int bulletsPerShot = 1;
    [Min(0f)] public float spread = 1.5f;

    [Header("Optional Visuals")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootSfx;
    public AudioClip reloadSfx;
}