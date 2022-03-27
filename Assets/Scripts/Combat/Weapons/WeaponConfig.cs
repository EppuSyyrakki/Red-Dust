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
			Projectile projectile, Effect[] effects, Weapon prefab)
		{
			this.name = name;
			this.damage = damage;
			this.range = range;
			this.penetration = penetration;
			this.projectile = projectile;
			this.effects = effects;
			this.prefab = prefab;
		}

		public readonly string name;
		public int damage;
		public float range;		
		public int penetration;        
		public readonly Projectile projectile;
		public readonly Effect[] effects;
		public readonly Weapon prefab;	
	}

	[CreateAssetMenu(fileName = "New Weapon Config", menuName = "Red Dust/New Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
		[SerializeField]
		private string Name;

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

		public Weapon Create(Transform rHand, Transform lHand)
		{
			// DestroyOldWeapon(rHand);
			var data = new WeaponData(Name, damage, range, penetration, projectilePrefab, effects, weaponPrefab);
			Weapon wpn = Instantiate(data.prefab, rHand);
			var muzzle = wpn.transform.FindObjectWithTag(Values.Tag.Muzzle);
			wpn.Set(data, rHand, lHand, muzzle.transform);
			return wpn;
		}

		private void DestroyOldWeapon(Transform hand)
		{
			Transform oldWeapon = hand.FindObjectWithTag(Values.Tag.Weapon).transform;
			Transform oldMuzzle = hand.FindObjectWithTag(Values.Tag.Muzzle).transform;
			oldWeapon.name = "OLD WEAPON";
			oldMuzzle.name = "OLD MUZZLE";
			Destroy(oldWeapon.gameObject);
		}
	}
}