using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseFollow : MonoBehaviour
{
  Vector3 look_ahead;

  public Material grid_material;
  public Vector3 lazy_origin_ray;

  new GameObject camera;

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
  GameObject ground;

  int origin_ray_id;
  int camera_position_id;

  void Start()
  {
    look_ahead  = new Vector3(0,0,1);
    lazy_origin_ray = new Vector3(0,0,1);

    camera = GameObject.Find("Main Camera");

    zoom_cur = 0;
    zoom_target = 0;
    zoom_t = 0;
    zoom_object = new GameObject[3];
    zoom_scale = new float[3];
    zoom_offset = new Vector3[3];
    zoom_object[0] = GameObject.Find("Zoom0");
    zoom_object[1] = GameObject.Find("Zoom1");
    zoom_object[2] = GameObject.Find("Zoom2");
    zoom_scale[0] = 1;
    zoom_scale[1] = 10;
    zoom_scale[2] = 100;
    zoom_offset[0] = new Vector3(0,1,0);
    zoom_offset[1] = new Vector3(10,11,10);
    zoom_offset[2] = new Vector3(100,101,100);

    zoom_scale_cur = zoom_scale[zoom_cur];
    zoom_offset_cur = zoom_offset[zoom_cur];

    GameObject dome = GameObject.Find("DomeGrid");
    dome_collider = dome.GetComponent<Collider>();
    ground = GameObject.Find("Ground");

    origin_ray_id = Shader.PropertyToID("lazy_origin_ray");
    camera_position_id = Shader.PropertyToID("cam_position");
  }

  void Update()
  {
    if(zoom_t == 0 && Input.GetMouseButtonDown(0))
    {
      zoom_t = 0.01f;
      zoom_target = 1;
    }

    if(zoom_t > 0)
    {
      zoom_t += 0.01f;

      if(zoom_t > 1)
      {
        zoom_t = 0;
        zoom_cur = zoom_target;
        if(zoom_cur == 1) ground.SetActive(false);
      }

      zoom_scale_cur = Mathf.Lerp(zoom_scale[zoom_cur],zoom_scale[zoom_target],zoom_t);
      zoom_offset_cur = Vector3.Lerp(zoom_offset[zoom_cur],zoom_offset[zoom_target],zoom_t);
    }

    zoom_object[zoom_cur].transform.localScale = new Vector3(zoom_scale_cur,zoom_scale_cur,zoom_scale_cur);
    transform.position = zoom_offset_cur;

    transform.rotation = Quaternion.Euler((Input.mousePosition.y-Screen.height/2)/-2, (Input.mousePosition.x-Screen.width/2)/2, 0);

    Vector3 backtrack = camera.transform.rotation * look_ahead * 20;
    Ray ray = new Ray(camera.transform.position+backtrack, camera.transform.rotation*-look_ahead);
    RaycastHit hit;
    if(dome_collider.Raycast(ray, out hit, 100.0F))
    {
      lazy_origin_ray = Vector3.Normalize(Vector3.Lerp(lazy_origin_ray, Vector3.Normalize(ray.GetPoint(hit.distance)), 0.01f));
      grid_material.SetVector(camera_position_id,camera.transform.position);
      grid_material.SetVector(origin_ray_id,lazy_origin_ray);
    }
  }

}

