using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelWrangler : MonoBehaviour {

	public GameObject LabelPrefab;
	new GameObject camera;
	CameraMouseFollow cameraScript;
	const int s = 10;
	GameObject[] labels;
	TextMesh[] labelsTexts;
	GameObject pointLabel;
	TextMesh pointLabelText;
	GameObject primaryLabel;
	TextMesh primaryLabelText;

	//GameObject dome;
	//GameObject plane;

	void Start ()
	{
		camera = GameObject.Find ("CameraParent");
		cameraScript = camera.GetComponent<CameraMouseFollow>();

		labels = new GameObject[s*s];
		labelsTexts = new TextMesh[s*s];

		for (int i = 0; i < s; i++)
		{
			for (int j = 0; j < s; j++)
			{
				labels[i*s + j] = (GameObject)Instantiate (LabelPrefab);
				labelsTexts[i*s + j] = labels[i*s + j].GetComponent<TextMesh>();
			}
		}
		pointLabel = (GameObject)Instantiate(LabelPrefab);
		pointLabelText = pointLabel.GetComponent<TextMesh>();
		pointLabelText.text = "x";
		primaryLabel = (GameObject)Instantiate(LabelPrefab);
		primaryLabelText = primaryLabel.GetComponent<TextMesh>();

		//dome = GameObject.Find("DomeGrid");
		//plane = GameObject.Find("PlaneGrid");
	}
	
	void Update()
	{
		float dome_s = 5;
		float x2;
		float y2;
		float z2;

		Vector3 gaze_position = cameraScript.lazy_origin_ray*dome_s;

		x2 = gaze_position.x;
		x2 *= x2;
		y2 = gaze_position.y;
		y2 *= y2;
		z2 = gaze_position.z;
		z2 *= z2;
		float plane_dist_from_orig = Mathf.Sqrt(x2+z2);
		//float dist_from_orig = Mathf.Sqrt(x2+y2+z2);
		float pitch = Mathf.Atan2(gaze_position.y,plane_dist_from_orig)/Mathf.PI*180; //pitch is angle of elevation
		float yaw   = Mathf.Atan2(gaze_position.z,gaze_position.x)     /Mathf.PI*180; //yaw is angle of cardinal direction

		pointLabel.transform.position = gaze_position;
		pointLabel.transform.rotation = Quaternion.Euler(-pitch,90f-yaw,0);
		primaryLabel.transform.position = pointLabel.transform.position+new Vector3(0f,0.5f,0f);
		primaryLabel.transform.rotation = pointLabel.transform.rotation;
		primaryLabelText.text = string.Format("{0}°,{1}°", pitch.ToString("F1"), yaw.ToString("F1"));
	}
}
