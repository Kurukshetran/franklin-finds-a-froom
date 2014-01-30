using UnityEngine;
using System.Collections;

public class DirectionController : MonoBehaviour {

	/**
	 * Checks for known script controllers, in the object this script it attached to, 
	 * that should support the SetDirection() method and passes on the direction.
	 */
	public void SetDirection(bool dir) {
		EnemyController enemy = GetComponent<EnemyController>();
		if (enemy) {
			enemy.SetDirection(dir);
		}

		CoinController coin = GetComponent<CoinController>();
		if (coin) {
			coin.SetDirection(dir);
		}
	}
}
