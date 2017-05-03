﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScript : MonoBehaviour
{
  Vector3 look_ahead;
  Vector3 origin_pt;
  Vector3 origin_ray;
  Vector3 lazy_origin_ray;
  Vector2 lazy_origin_euler;
  Vector3 snapped_lazy_origin_ray;
  Vector2 snapped_lazy_origin_euler;

  int camera_position_id;
  int lazy_origin_ray_id;
  int snapped_lazy_origin_pitch_id;
  int snapped_lazy_origin_yaw_id;
  int grid_resolution_id;
  int grid_alpha_id;
  Vector3 zoom_target;

  //unity-set
  public Material grid_material;
  public GameObject label_prefab;
  public GameObject star_prefab;

  //objects
  GameObject camera_house;
  new GameObject camera;
  Vector3 old_cam_position;
  GameObject dome;
  GameObject plane;
  GameObject ground;

  //zoom
  int n_zooms;
  int zoom_cur;
  int zoom_next;
  float zoom_t;
  GameObject[] zoom_cluster;
  float[,] zoom_cluster_zoom;
  float[] zoom_grid_resolution;
  Vector2[] zoom_target_euler; //yaw/pitch
  Vector2[] zoom_target_inflated_euler; //yaw/pitch (artificially corrected to higher fidelity)
  float[] zoom_target_euler_inflation;

  float zoom_grid_resolution_cur;
  float grid_alpha;

  Collider plane_collider;

  //labels
  GameObject pointLabel;
  TextMesh pointLabelText;
  GameObject snapPointLabel;
  TextMesh snapPointLabelText;
  GameObject primaryLabel;
  TextMesh primaryLabelText;
  GameObject earthLabel;
  TextMesh earthLabelText;
  GameObject earthDistLabel;
  TextMesh earthDistLabelText;

  void Start()
  {
    look_ahead  = new Vector3(0,0,1);

    origin_pt = look_ahead;
    origin_ray = look_ahead;
    lazy_origin_ray = look_ahead;
    lazy_origin_euler = new Vector2(0,0);
    snapped_lazy_origin_ray = look_ahead;
    snapped_lazy_origin_euler = new Vector2(0,0);

    camera_position_id = Shader.PropertyToID("cam_position");
    lazy_origin_ray_id = Shader.PropertyToID("lazy_origin_ray");
    snapped_lazy_origin_pitch_id = Shader.PropertyToID("snapped_lazy_origin_pitch");
    snapped_lazy_origin_yaw_id = Shader.PropertyToID("snapped_lazy_origin_yaw");
    grid_resolution_id = Shader.PropertyToID("grid_resolution");
    grid_alpha_id = Shader.PropertyToID("grid_alpha");

    //objects
    camera_house = GameObject.Find("CameraHouse");
    camera       = GameObject.Find("Main Camera");
    old_cam_position = camera_house.transform.position;
    dome   = GameObject.Find("DomeGrid");
    plane  = GameObject.Find("PlaneGrid");
    ground = GameObject.Find("Ground");
    ground.GetComponent<Renderer>().material.SetColor("_Color",Color.white);

    //zoom
    n_zooms = 4;
    zoom_cur = 0;
    zoom_next = 0;
    zoom_t = 0;
    zoom_cluster = new GameObject[n_zooms];
    zoom_cluster_zoom = new float[n_zooms,n_zooms];
    zoom_target_euler = new Vector2[n_zooms];
    zoom_target_inflated_euler = new Vector2[n_zooms];
    zoom_target_euler_inflation = new float[n_zooms];
    zoom_grid_resolution = new float[n_zooms];
    zoom_cluster[0] = GameObject.Find("Zoom0Cluster");
    zoom_cluster[1] = GameObject.Find("Zoom1Cluster");
    zoom_cluster[2] = GameObject.Find("Zoom2Cluster");
    for(int i = 3; i < n_zooms; i++)
      zoom_cluster[i] = new GameObject();

    // earth
    zoom_cluster_zoom[0,0] = 1f;         //on earth
    zoom_cluster_zoom[0,1] = 0.00001f;   //on solar system
    zoom_cluster_zoom[0,2] = 0.0000001f; //on galaxy
    zoom_cluster_zoom[0,3] = 0.0000001f; //on beyond
    // solar system
    zoom_cluster_zoom[1,0] = 1f;
    zoom_cluster_zoom[1,1] = 1f;
    zoom_cluster_zoom[1,2] = 1f;
    zoom_cluster_zoom[1,3] = 1f;
    // galaxy
    zoom_cluster_zoom[2,0] = 1f;
    zoom_cluster_zoom[2,1] = 1f;
    zoom_cluster_zoom[2,2] = 0.01f;
    zoom_cluster_zoom[2,3] = 0.01f;
    // beyond
    zoom_cluster_zoom[3,0] = 1f;
    zoom_cluster_zoom[3,1] = 1f;
    zoom_cluster_zoom[3,2] = 0.01f;
    zoom_cluster_zoom[3,3] = 0.01f;

    for(int i = 0; i < n_zooms; i++)
    {
      zoom_target_euler[i] = Vector2.zero;
      zoom_target_inflated_euler[i] = Vector2.zero;
    }
    zoom_target_euler_inflation[0] = 1f;
    zoom_target_euler_inflation[1] = 5f;
    zoom_target_euler_inflation[2] = 10f;
    zoom_target_euler_inflation[3] = 15f;
    zoom_grid_resolution[0] = 10f;
    zoom_grid_resolution[1] = 5f;
    zoom_grid_resolution[2] = 1f;
    zoom_grid_resolution[3] = 0.5f;

    GameObject[] star_groups;
    GameObject star;
    Vector3 starpos;

    int n_stars = 50000;
    int n_groups = (int)Mathf.Ceil(n_stars/1000);
    int n_stars_in_group;
    star_groups = new GameObject[n_groups];
    star = (GameObject)Instantiate(star_prefab);

    for(int i = 0; i < n_groups; i++)
    {
      n_stars_in_group = Mathf.Min(1000,n_stars);
      CombineInstance[] combine = new CombineInstance[n_stars_in_group];

      for(int j = 0; j < n_stars_in_group; j++)
      {
        bool good_star = false;
        starpos = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f));
        good_star = (starpos.sqrMagnitude < Random.Range(0f,1f));
        while(!good_star)
        {
          starpos = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f));
          good_star = (starpos.sqrMagnitude < Random.Range(0f,1f));
        }
        starpos = Vector3.Normalize(starpos);
        starpos *= Random.Range(200f,820f);

        star.transform.position = starpos;
        star.transform.rotation = Quaternion.Euler(Random.Range(0f,360f),Random.Range(0f,360f),Random.Range(0f,360f));
        star.transform.localScale = new Vector3(0.5f,0.5f,0.5f);

        combine[j].mesh = star.transform.GetComponent<MeshFilter>().mesh;
        combine[j].transform = star.transform.localToWorldMatrix;
      }

      star_groups[i] = (GameObject)Instantiate(star_prefab);
      star_groups[i].transform.position = new Vector3(0,0,0);
      star_groups[i].transform.rotation = Quaternion.Euler(0,0,0);
      star_groups[i].transform.localScale = new Vector3(1,1,1);
      star_groups[i].transform.SetParent(zoom_cluster[2].transform,false);
      star_groups[i].transform.GetComponent<MeshFilter>().mesh = new Mesh();
      star_groups[i].transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

      n_stars -= n_stars_in_group;
    }


    zoom_target = Vector3.zero;

    zoom_grid_resolution_cur = zoom_grid_resolution[zoom_cur];
    grid_alpha = 1f;

    plane_collider = plane.GetComponent<Collider>();

    pointLabel = (GameObject)Instantiate(label_prefab);
    pointLabelText = pointLabel.GetComponent<TextMesh>();
    pointLabelText.text = "x";
    snapPointLabel = (GameObject)Instantiate(label_prefab);
    snapPointLabelText = snapPointLabel.GetComponent<TextMesh>();
    snapPointLabelText.text = "o";
    primaryLabel = (GameObject)Instantiate(label_prefab);
    primaryLabelText = primaryLabel.GetComponent<TextMesh>();
    earthLabel = (GameObject)Instantiate(label_prefab);
    earthLabelText = earthLabel.GetComponent<TextMesh>();
    earthDistLabel = (GameObject)Instantiate(label_prefab);
    earthDistLabelText = earthDistLabel.GetComponent<TextMesh>();
  }

  void Update()
  {
    float dome_s = 5;

    if(zoom_t == 0 && Input.GetMouseButtonDown(0))
    {
      zoom_t = 0.01f;
      zoom_next = (zoom_next+1)%n_zooms;
      if(zoom_next == 0)
      {
        //zoom_target_euler[zoom_cur] = new Vector2(0,0); //don't change
        zoom_target = new Vector3(0,0,0);
      }
      else
      {
        zoom_target_euler[zoom_cur] = snapped_lazy_origin_euler;
        zoom_target = Quaternion.Euler(-Mathf.Rad2Deg*zoom_target_euler[zoom_cur].x, -Mathf.Rad2Deg*zoom_target_euler[zoom_cur].y+90, 0) * look_ahead * Mathf.Pow(10,zoom_next);

        if(zoom_cur == 0)
        {
          zoom_target_inflated_euler[zoom_cur] = zoom_target_euler[zoom_cur];
        }
        else
        {
          zoom_target_inflated_euler[zoom_cur] = zoom_target_inflated_euler[zoom_cur-1]+((zoom_target_euler[zoom_cur]-zoom_target_euler[zoom_cur-1])/zoom_target_euler_inflation[zoom_cur]);
        }
      }
      if(zoom_next == 1) ground.SetActive(false);

      if(zoom_next != 0)
      {
        plane.transform.position = zoom_target + Vector3.Normalize(zoom_target)*(dome_s*zoom_next);
        plane.transform.rotation = Quaternion.Euler(-zoom_target_euler[zoom_cur].x*Mathf.Rad2Deg+90,-zoom_target_euler[zoom_cur].y*Mathf.Rad2Deg+90,0);//+90+180,0);
      }
      //float s = 2*(zoom_target.magnitude+dome_s);
      //dome.transform.localScale = new Vector3(s,s,s);
    }

    if(zoom_t > 0)
    {
      zoom_t += 0.05f;

      if(zoom_t > 1)
      {
        zoom_t = 0;
        camera_house.transform.position = zoom_target + new Vector3(0,1,0);
        old_cam_position = camera_house.transform.position;
        zoom_cur = zoom_next;
        if(zoom_next == 0)
        {
          ground.SetActive(true);
          plane.transform.position = new Vector3(0,0,0);
        }
      }

      for(int i = 0; i < n_zooms; i++)
      {
        float s = Mathf.Lerp(zoom_cluster_zoom[i,zoom_cur],zoom_cluster_zoom[i,zoom_next],zoom_t);
        zoom_cluster[i].transform.localScale = new Vector3(s,s,s);
      }

      zoom_grid_resolution_cur = Mathf.Lerp(zoom_grid_resolution[zoom_cur],zoom_grid_resolution[zoom_next],zoom_t);

      camera_house.transform.position = Vector3.Lerp(old_cam_position, zoom_target + new Vector3(0,1,0), zoom_t);

      grid_alpha = ((zoom_t-0.5f)*2f);
      grid_alpha = grid_alpha*grid_alpha*grid_alpha*grid_alpha;
      grid_alpha *= grid_alpha;
    }
    else
    {
      for(int i = 0; i < n_zooms; i++)
      {
        float s = zoom_cluster_zoom[i,zoom_cur];
        zoom_cluster[i].transform.localScale = new Vector3(s,s,s);
      }
    }

    camera_house.transform.rotation = Quaternion.Euler((Input.mousePosition.y-Screen.height/2)/-2, (Input.mousePosition.x-Screen.width/2)/2, 0);

    Vector3 cast_vision = camera.transform.position + (camera.transform.rotation * look_ahead * (dome_s+1));
    if(zoom_cur == 0 && zoom_t < 0.5)
    {
      origin_pt = cast_vision;
      origin_ray = Vector3.Normalize(origin_pt);
    }
    else
    {
      Ray ray = new Ray(Vector3.zero, Vector3.Normalize(cast_vision));
      RaycastHit hit;
      if(plane_collider.Raycast(ray, out hit, 10000.0F))
      {
        origin_pt = hit.point;
        origin_ray = Vector3.Normalize(origin_pt);
      }
    }

    lazy_origin_ray = Vector3.Normalize(Vector3.Lerp(lazy_origin_ray, origin_ray, 0.01f));

    Vector3 lazy_origin_ray_sqr = lazy_origin_ray;
    lazy_origin_ray_sqr.x *= lazy_origin_ray_sqr.x;
    lazy_origin_ray_sqr.y *= lazy_origin_ray_sqr.y;
    lazy_origin_ray_sqr.z *= lazy_origin_ray_sqr.z;
    float lazy_plane_origin_dist = Mathf.Sqrt(lazy_origin_ray_sqr.x+lazy_origin_ray_sqr.z);

    float grid_resolution = zoom_grid_resolution_cur;
    lazy_origin_euler.x = Mathf.Atan2(lazy_origin_ray.y,lazy_plane_origin_dist);
    lazy_origin_euler.y = Mathf.Atan2(lazy_origin_ray.z,lazy_origin_ray.x);
    snapped_lazy_origin_euler.x = ((Mathf.Floor((lazy_origin_euler.x*Mathf.Rad2Deg)/grid_resolution)*grid_resolution)+grid_resolution/2)*Mathf.Deg2Rad;
    snapped_lazy_origin_euler.y = ((Mathf.Floor((lazy_origin_euler.y*Mathf.Rad2Deg)/grid_resolution)*grid_resolution)+grid_resolution/2)*Mathf.Deg2Rad;

    snapped_lazy_origin_ray = Quaternion.Euler(-Mathf.Rad2Deg*snapped_lazy_origin_euler.x, -Mathf.Rad2Deg*snapped_lazy_origin_euler.y+90, 0) * look_ahead;

    //labels
    Vector3         lazy_gaze_position;
    Vector3 snapped_lazy_gaze_position;

    if(zoom_cur == 0 && zoom_t < 0.5)
    {
              lazy_gaze_position =         lazy_origin_ray*dome_s;
      snapped_lazy_gaze_position = snapped_lazy_origin_ray*dome_s;
    }
    else
    {
      Ray ray = new Ray(Vector3.zero, lazy_origin_ray);
      RaycastHit hit;
      if(plane_collider.Raycast(ray, out hit, 10000.0F))
      {
        lazy_gaze_position = hit.point;
      }
      else
      {
        lazy_gaze_position = lazy_origin_ray*plane.transform.position.magnitude;
      }

      ray = new Ray(Vector3.zero, snapped_lazy_origin_ray);
      if(plane_collider.Raycast(ray, out hit, 10000.0F))
      {
        snapped_lazy_gaze_position = hit.point*plane.transform.position.magnitude;
      }
      else
      {
        snapped_lazy_gaze_position = snapped_lazy_origin_ray*plane.transform.position.magnitude;
      }
    }

        pointLabel.transform.position =         lazy_gaze_position;
    snapPointLabel.transform.position = snapped_lazy_gaze_position;
        pointLabel.transform.rotation = Quaternion.Euler(-        lazy_origin_euler.x*Mathf.Rad2Deg,90f-        lazy_origin_euler.y*Mathf.Rad2Deg,0);
    snapPointLabel.transform.rotation = Quaternion.Euler(-snapped_lazy_origin_euler.x*Mathf.Rad2Deg,90f-snapped_lazy_origin_euler.y*Mathf.Rad2Deg,0);

    primaryLabel.transform.position = pointLabel.transform.position+new Vector3(0f,0.5f,0f);
    primaryLabel.transform.rotation = pointLabel.transform.rotation;

    if(zoom_cur != 0)
    {
      earthLabel.transform.position = camera_house.transform.position.normalized * (camera_house.transform.position.magnitude-dome_s);
      earthLabel.transform.rotation = Quaternion.Euler(lazy_origin_euler.x*Mathf.Rad2Deg,270f-lazy_origin_euler.y*Mathf.Rad2Deg,0);
      earthLabelText.text = "Earth";
      earthDistLabel.transform.position = earthLabel.transform.position;
      earthDistLabel.transform.rotation = earthLabel.transform.rotation;
      earthDistLabelText.text = string.Format("{0} mi",camera_house.transform.position.magnitude*camera_house.transform.position.magnitude);
    }

    Vector2 lazy_origin_inflated_euler = lazy_origin_euler;
    if(zoom_cur != 0) lazy_origin_inflated_euler = zoom_target_inflated_euler[zoom_cur-1]+((lazy_origin_euler-zoom_target_euler[zoom_cur-1])/zoom_target_euler_inflation[zoom_cur]);
    lazy_origin_inflated_euler *= Mathf.Rad2Deg;

    primaryLabelText.text = string.Format("{0}°,{1}°", lazy_origin_inflated_euler.y.ToString("F2"), lazy_origin_inflated_euler.x.ToString("F2"));

    //shader inputs
    grid_material.SetVector(camera_position_id,camera.transform.position);
    grid_material.SetVector(lazy_origin_ray_id,lazy_origin_ray);
    grid_material.SetFloat(snapped_lazy_origin_pitch_id,snapped_lazy_origin_euler.x);
    grid_material.SetFloat(snapped_lazy_origin_yaw_id,snapped_lazy_origin_euler.y);
    grid_material.SetFloat(grid_resolution_id,grid_resolution);
    grid_material.SetFloat(grid_alpha_id,grid_alpha);
  }

}
