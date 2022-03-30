using UnityEngine;

namespace RedDust.Motion
{
	public class RandomAnimation : StateMachineBehaviour
	{
		[SerializeField]
		private int clipCount;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (clipCount < 1)
			{
				Debug.LogWarning(animator.gameObject.name + " animator has an invalid clip count.");
				return;
			}

			int chosenClip = Random.Range(1, clipCount + 1);
			animator.SetInteger(Values.Animation.AnimationIndex, chosenClip);
		}
	}
}
