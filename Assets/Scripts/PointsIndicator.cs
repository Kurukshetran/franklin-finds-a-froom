using UnityEngine;
using System.Collections;

public class PointsIndicator : MonoBehaviour {

	public float smoothingValue;
	public Vector3 finalPosDelta;
	public float destroyTime;
	private Vector3 finalPos;
	private TextMesh textMesh;
	private bool doAnimation;

	// Reference to GameController
	private GameController gameController;

	#region Y position of each platform level
	public float level3YPos;
	public float level2YPos;
	public float level1YPos;
	public float level0YPos;
	#endregion

	// Text color for when points are being reduced
	public Color deductionColor;

	#region Multiplier for points accumulated at each level
	public float level3Multiplier = 1;
	public float level2Multiplier = 1;
	public float level1Multiplier = 1;
	public float level0Multiplier = 1;
	#endregion

	private void Awake() {
		textMesh = GetComponent<TextMesh>();
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}

	private void Update() {
		if (doAnimation) {
			float x = Mathf.Lerp(transform.position.x, finalPos.x, Time.deltaTime * smoothingValue);
			float y = Mathf.Lerp(transform.position.y, finalPos.y, Time.deltaTime * smoothingValue);

			transform.position = new Vector3(x, y, -1);
		}
	}

	public void AddPoints(int points, Vector3 startingPos) {
		// Set the starting position of this text
		transform.position = startingPos;

		// Set the points to display
		if (points > 0) {
			if (transform.position.y > level3YPos) {
				points = Mathf.CeilToInt(points * level3Multiplier);
			}
			else if (transform.position.y > level2YPos) {
				points = Mathf.CeilToInt(points * level2Multiplier);
			}
			else if (transform.position.y > level1YPos) {
				points = Mathf.CeilToInt(points * level1Multiplier);
			}
			else if (transform.position.y > level0YPos) {
				points = Mathf.CeilToInt(points * level0Multiplier);
			}

			textMesh.text = "+" + points;
		}
		else {
			textMesh.text = points.ToString();
			textMesh.color = deductionColor;
		}

		// Add points to game total
		gameController.AddToScore(points);

		// Determine final position
		float finalX = transform.position.x + finalPosDelta.x;
		float finalY = transform.position.y + finalPosDelta.y;
		float finalZ = transform.position.z + finalPosDelta.z;
		finalPos = new Vector3(finalX, finalY, finalZ);

		doAnimation = true;

		Destroy(this.gameObject, destroyTime);
	}
}
