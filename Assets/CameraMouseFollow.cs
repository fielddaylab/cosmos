using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseFollow : MonoBehaviour
{
  Vector3 look_ahead;

  public Material grid_material;
  public Vector3 lazy_origin_ray;

  new GameObject camera;
  Collider dome_collider;
  GameObject universe;
  GameObject ground;

  int origin_ray_id;
  int camera_position_id;

  float scale;
  Vector3 offset;
  float base_scale;
  Vector3 base_offset;
  float target_scale;
  Vector3 target_offset;
  float target_t;

  void Start()
  {
    look_ahead  = new Vector3(0,0,1);
    lazy_origin_ray = new Vector3(0,0,1);

    camera = GameObject.Find("Main Camera");
    GameObject dome = GameObject.Find("DomeGrid");
    dome_collider = dome.GetComponent<Collider>();
    universe = GameObject.Find("Universe");
    ground = GameObject.Find("Ground");

    origin_ray_id = Shader.PropertyToID("lazy_origin_ray");
    camera_position_id = Shader.PropertyToID("cam_position");

    offset = new Vector3(0,1,0);
    scale = 1;
    base_offset = new Vector3(0,1,0);
    base_scale = 1;
    target_offset = new Vector3(0,1,0);
    target_scale = 1;
    target_t = 0;
  }

  void Update()
  {
    if(target_t == 0 && Input.GetMouseButtonDown(0))
    {
      target_scale = 0.1f;
      target_offset.x = 10;
      target_offset.y = 11;
      target_offset.z = 10;
      target_t = 0.01f;
    }

    if(target_t > 0)
    {
      target_t += 0.01f;

      if(target_t > 1)
      {
        target_t = 0;
        offset.x = target_offset.x;
        offset.y = target_offset.y;
        offset.z = target_offset.z;
        scale = target_scale;
        base_offset.x = target_offset.x;
        base_offset.y = target_offset.y;
        base_offset.z = target_offset.z;
        base_scale = target_scale;
      }
      else
      {
        ground.SetActive(false);
        offset.x = Mathf.Lerp(base_offset.x,target_offset.x,target_t);
        offset.y = Mathf.Lerp(base_offset.y,target_offset.y,target_t);
        offset.z = Mathf.Lerp(base_offset.z,target_offset.z,target_t);
        scale = Mathf.Lerp(base_scale,target_scale,target_t);
      }
    }

    universe.transform.localScale = new Vector3(scale,scale,scale);
    transform.position = offset;

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

