using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScript : MonoBehaviour
{
  Vector3 look_ahead;
  Vector3 lazy_origin_ray;
  int origin_ray_id;
  int camera_position_id;

  //unity-set
  public Material dome_grid_material;
  public GameObject label_prefab;

  //objects
  GameObject camera_house;
  new GameObject camera;
  GameObject dome;
  GameObject plane;
  GameObject ground;
  GameObject earth;
  GameObject sun;
  GameObject dome_labels;

  //zoom
  int n_zooms;
  int zoom_cur;
  int zoom_target;
  float zoom_t;
  GameObject[] zoom_object;
  float[] zoom_scale;
  Vector3[] zoom_offset;
  float zoom_scale_cur;
  Vector3 zoom_offset_cur;

  //zoom 0 special
  Collider dome_collider;

  //labels
  const int label_grid_s = 3;
  GameObject[] labels;
  TextMesh[] labelsTexts;
  GameObject pointLabel;
  TextMesh pointLabelText;
  GameObject primaryLabel;
  TextMesh primaryLabelText;

  void zoom_objects()
  {
    for(int i = 0; i < n_zooms; i++)
    {
      float scale = zoom_scale[i]/zoom_scale_cur;
      zoom_object[i].transform.localScale = new Vector3(scale,scale,scale);
      zoom_object[i].transform.position = zoom_offset[i]*scale;
    }
  }

  void Start()
  {
    look_ahead  = new Vector3(0,0,1);
    lazy_origin_ray = new Vector3(0,0,1);

    origin_ray_id = Shader.PropertyToID("lazy_origin_ray");
    camera_position_id = Shader.PropertyToID("cam_position");

    //objects
    camera_house = GameObject.Find("CameraHouse");
	camera       = GameObject.Find("Main Camera");
    dome   = GameObject.Find("DomeGrid");
    plane  = GameObject.Find("PlaneGrid");
		plane.transform.rotation = plane.transform.rotation;
    ground = GameObject.Find("Ground");
    ground.GetComponent<Renderer>().material.SetColor("_Color",Color.red);
    earth  = GameObject.Find("Earth");
    earth.GetComponent<Renderer>().material.SetColor("_Color",Color.red);
    sun    = GameObject.Find("Sun");
    sun.GetComponent<Renderer>().material.SetColor("_Color",Color.green);
    dome_labels = GameObject.Find("DomeLabels");

    //zoom
    n_zooms = 3;
    zoom_cur = 0;
    zoom_target = 0;
    zoom_t = 0;
    zoom_object = new GameObject[3];
    zoom_scale = new float[3];
    zoom_offset = new Vector3[3];
    zoom_scale[0] = 1;
    zoom_scale[1] = 10;
    zoom_scale[2] = 100;
    zoom_object[0] = GameObject.Find("Zoom0");
    zoom_object[1] = GameObject.Find("Zoom1");
    zoom_object[2] = GameObject.Find("Zoom2");
    zoom_offset[0] = zoom_object[0].transform.position;
    zoom_offset[1] = zoom_object[1].transform.position;
    zoom_offset[2] = zoom_object[2].transform.position;

    zoom_scale_cur = zoom_scale[zoom_cur];
    zoom_offset_cur = zoom_offset[zoom_cur];

    zoom_objects();

    //zoom 0 special
    dome_collider = dome.GetComponent<Collider>();

    //labels
    labels = new GameObject[label_grid_s*label_grid_s];
    labelsTexts = new TextMesh[label_grid_s*label_grid_s];

    for (int i = 0; i < label_grid_s; i++)
    {
      for (int j = 0; j < label_grid_s; j++)
      {
        labels[i*label_grid_s + j] = (GameObject)Instantiate(label_prefab);
        labels[i*label_grid_s + j].transform.parent = dome_labels.transform;
        labelsTexts[i*label_grid_s + j] = labels[i*label_grid_s + j].GetComponent<TextMesh>();
      }
    }
    pointLabel = (GameObject)Instantiate(label_prefab);
    pointLabel.transform.parent = dome_labels.transform;
    pointLabelText = pointLabel.GetComponent<TextMesh>();
    pointLabelText.text = "x";
    primaryLabel = (GameObject)Instantiate(label_prefab);
    primaryLabel.transform.parent = dome_labels.transform;
    primaryLabelText = primaryLabel.GetComponent<TextMesh>();
  }

  void Update()
  {
    if(zoom_t == 0 && Input.GetMouseButtonDown(0))
    {
      zoom_t = 0.01f;
      zoom_target = (zoom_target+1)%n_zooms;
           if(zoom_target == 1) ground.SetActive(false);
    }

    if(zoom_t > 0)
    {
      zoom_t += 0.01f;

      if(zoom_t > 1)
      {
        zoom_t = 0;
        zoom_cur = zoom_target;
		if(zoom_target == 0) ground.SetActive(true);
      }

      zoom_scale_cur = Mathf.Lerp(zoom_scale[zoom_cur],zoom_scale[zoom_target],zoom_t);
      zoom_offset_cur = Vector3.Lerp(zoom_offset[zoom_cur],zoom_offset[zoom_target],zoom_t);
    }

    zoom_objects();

    camera_house.transform.position = zoom_offset_cur+new Vector3(0,1,0);
    camera_house.transform.rotation = Quaternion.Euler((Input.mousePosition.y-Screen.height/2)/-2, (Input.mousePosition.x-Screen.width/2)/2, 0);

    Vector3 backtrack = camera.transform.rotation * look_ahead * 20;
    Ray ray = new Ray(camera.transform.position+backtrack, camera.transform.rotation*-look_ahead);
    RaycastHit hit;
    if(dome_collider.Raycast(ray, out hit, 100.0F))
    {
      lazy_origin_ray = Vector3.Normalize(Vector3.Lerp(lazy_origin_ray, Vector3.Normalize(ray.GetPoint(hit.distance)), 0.01f));
      dome_grid_material.SetVector(camera_position_id,camera.transform.position);
      dome_grid_material.SetVector(origin_ray_id,lazy_origin_ray);
    }

    //labels
    float dome_s = 5;
    Vector3 gaze_position = lazy_origin_ray*dome_s;

    float x2 = gaze_position.x; x2 *= x2;
    float y2 = gaze_position.y; y2 *= y2;
    float z2 = gaze_position.z; z2 *= z2;
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
