using UnityEngine;
using System.Collections;

public class ExampleAlert : MonoBehaviour
{
    public Transform exampleCube;
    private Vector3 cubeStartPos;

	// Use this for initialization
	void Start ()
	{
	    cubeStartPos = exampleCube.transform.position;
	    StartCoroutine(ExampleAlertStuff());
	}

    IEnumerator ExampleAlertStuff()
    {
        while (gameObject.activeInHierarchy)
        {
            //Show alert following cube.
            AlertManager.AlertWorld("Wheeeee!!!", exampleCube, Color.cyan, 4f);
            yield return new WaitForSeconds(2);
            AlertManager.Alert("This is an alert in the middle!", Color.red, AlertType.Middle, 3f);
            yield return new WaitForSeconds(4);
            AlertManager.Alert("This is an alert in the top!", Color.white, AlertType.Top, 3f);
            yield return new WaitForSeconds(1f);
            AlertManager.Alert("And another one before previous one was over!", Color.cyan, AlertType.Top, 3f);
            yield return new WaitForSeconds(1f);
            AlertManager.AlertWorld("Cubey was here!", exampleCube.position, Color.cyan, 4f, 1.45f);
            yield return new WaitForSeconds(3f);
            AlertManager.Alert("And one in the bottom for good measure!", Color.red, AlertType.Bottom, 3f);
            yield return new WaitForSeconds(2.5f);
        }
    }

    protected void Update()
    {
        float distanceUp = 10 * Mathf.Sin(Time.timeSinceLevelLoad * 0.80f);
        float distanceSide = 10 * Mathf.Sin(Time.timeSinceLevelLoad * 1.25f);

        exampleCube.position = cubeStartPos + (Vector3.up* distanceUp) + (Vector3.right* distanceSide);
    }
}
