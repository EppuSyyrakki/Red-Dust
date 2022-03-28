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
		Throwable = 5,
		Explosive = 6
	}

	[CreateAssetMenu(fileName = "New Weapon Config", menuName = "Red Dust/New Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
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

		[SerializeField]
		private AnimatorOverrideController animatorOverride;

		public Weapon Create(Transform rHand, Transform lHand)
		{
			DestroyOldWeapon(rHand);
			var data = new WeaponData(Name, damage, range, penetration, projectilePrefab, effects, weaponPrefab, type);
			Weapon wpn = Instantiate(data.prefab, rHand);
			Transform muzzle = wpn.transform.FindObjectWithTag(Values.Tag.Muzzle).transform;
			wpn.Set(data, rHand, lHand, muzzle, animatorOverride);
			return wpn;
		}

		private void DestroyOldWeapon(Transform hand)
		{
			var oldWeapon = hand.FindObjectWithTag(Values.Tag.Weapon);
			
			if (oldWeapon == null) { return; }

			var oldMuzzle = hand.FindObjectWithTag(Values.Tag.Muzzle);
			oldWeapon.name = "OLD WEAPON";
			oldMuzzle.name = "OLD MUZZLE";
			Destroy(oldWeapon);
		}
	}
}