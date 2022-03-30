using System;
using UnityEditor;
using UnityEngine;
using Utils;

namespace RedDust.Combat.Weapons
{
    [Serializable]
    public struct WeaponData
	{
		public WeaponData(string name, int damage, float range, int penetration, 
			Projectile projectile, Effect[] effects, Weapon prefab, WeaponType type)
		{
			this.name = name;
			this.damage = damage;
			this.range = range;
			this.penetration = penetration;
			this.projectile = projectile;
			this.effects = effects;
			this.prefab = prefab;
			this.type = type;
		}

		public readonly string name;
		public int damage;
		public float range;		
		public int penetration;        
		public readonly Projectile projectile;
		public readonly Effect[] effects;
		public readonly Weapon prefab;
		public readonly WeaponType type;
	}
	public enum WeaponType
	{
		Unarmed = 0,
		SmallMelee = 1,
		LargeMelee = 2,
		SmallGun = 3,
		LargeGun = 4,
		Throwable = 5
	}

	[CreateAssetMenu(fileName = "New Weapon Config", menuName = "Red Dust/New Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
		private const string TAG_WEAPON = Values.Tag.Weapon;
		private const string TAG_MUZZLE = Values.Tag.Muzzle;

		[SerializeField]
		private string Name;

		[SerializeField]
		private WeaponType type;

		[SerializeField]
		private int damage;

		[SerializeField, Range(0, Values.Combat.MaxProjectileTravel)]
		private float range;

		[SerializeField]
		private int penetration;

		[SerializeField]
		private Projectile projectilePrefab;

		[SerializeField]
		private Effect[] effects;

		[SerializeField]
		private Weapon weaponPrefab;

		/// <summary>
		/// Spawns this weapon as a child of the hand parameter.
		/// </summary>
		/// <returns>The spawned weapon.</returns>
		public Weapon Create(Transform hand)
		{
			// TODO: Must destroy old weapon, but also avoid null ref in the aiming rig constraints
			var data = new WeaponData(Name, damage, range, penetration, projectilePrefab, effects, weaponPrefab, type);
			Weapon wpn = Instantiate(data.prefab, hand);
			Transform muzzle = wpn.transform.FindObjectWithTag(Values.Tag.Muzzle).transform;
			wpn.Set(data, muzzle);
			return wpn;
		}

		public static void DestroyOldWeapon(Transform hand)
		{
			var oldWeapon = hand.FindObjectWithTag(TAG_WEAPON);
			
			if (oldWeapon == null) { return; }

			var oldMuzzle = hand.FindObjectWithTag(TAG_MUZZLE);
			oldWeapon.name = "OLD WEAPON";
			oldMuzzle.name = "OLD MUZZLE";
			Destroy(oldWeapon);
		}
	}
}