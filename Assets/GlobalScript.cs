using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScript : MonoBehaviour
{
  Vector3 look_ahead;
  Vector3 origin_to_pt;
  Vector3 origin_ray;
  Vector3 lazy_origin_ray;
  Vector2 lazy_origin_euler;
  Vector3 snapped_lazy_origin_ray;
  Vector2 snapped_lazy_origin_euler;
  Vector2 lazy_origin_inflated_euler;
  Vector3 cast_vision;
  Vector3 look_dir;

  float dome_s = 5;
  int n_projects = 5;

  int camera_position_id;
  int lazy_origin_ray_id;
  int snapped_lazy_origin_pitch_id;
  int snapped_lazy_origin_yaw_id;
  int grid_resolution_id;
  int grid_alpha_id;
  int ray_alpha_id;

  //unity-set
  public Material grid_material;
  public Material project_grid_material;
  public Material ray_material;
  public GameObject label_prefab;
  public GameObject star_prefab;
  public GameObject dome_project_prefab;
  public GameObject plane_project_prefab;

  //objects
  GameObject[] zoom_cluster;

  GameObject camera_house;
  new GameObject camera;

  GameObject dome;
  GameObject plane;
  GameObject ground;
  GameObject eyeray;

  GameObject[] dome_project;
  GameObject[] plane_project;

  GameObject blackhole;
  GameObject galaxy;
  GameObject solarsystem;
  GameObject earth;

  Vector3 player_height;

  //zoom
  int n_zooms;
  int zoom_cur;
  int zoom_next;

  //lerp vals
  float zoom_t;
  Vector3 player_position_from;
  Vector3 player_position_to;
  float[] zoom_cluster_zoom_from;
  float[] zoom_cluster_zoom_to;
  Vector3 blackhole_position_from;
  Vector3 blackhole_position_to;
  float zoom_grid_resolution_from;
  float zoom_grid_resolution_to;
  float zoom_grid_display_resolution_from;
  float zoom_grid_display_resolution_to;

  //lerp data
  float[,] zoom_cluster_zoom;
  float[] zoom_grid_resolution;
  float[] zoom_grid_display_resolution;

  //lerp result
  float zoom_grid_resolution_cur;
  float zoom_grid_display_resolution_cur;

  //zoom state
  Vector2[] zoom_target_euler; //yaw/pitch
  Vector2[] zoom_target_inflated_euler; //yaw/pitch (artificially corrected to higher fidelity)
  float[] zoom_target_euler_inflation;

  float grid_alpha;
  float ray_alpha;

  float goal_yaw;
  float goal_pitch;

  Collider plane_collider;

  //labels
  GameObject primaryLabel;
  TextMesh primaryLabelText;
  GameObject earthLabel;
  TextMesh earthLabelText;
  GameObject hudLabel;
  TextMesh hudLabelText;

  void Start()
  {
    look_ahead  = new Vector3(0,0,1);
    player_height = new Vector3(0,1.5f,0);

    origin_to_pt = look_ahead;
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
    ray_alpha_id = Shader.PropertyToID("ray_alpha");

    //objects
    camera_house  = GameObject.Find("CameraHouse");
    camera        = GameObject.Find("Main Camera");
    dome          = GameObject.Find("DomeGrid");
    plane         = GameObject.Find("PlaneGrid");
    ground = GameObject.Find("Ground");
    eyeray = GameObject.Find("Ray");

    dome.transform.localScale         = new Vector3(dome_s*2,dome_s*2,dome_s*2);
    dome_project = new GameObject[n_projects];
    for(int i = 0; i < n_projects; i++)
    {
      //dome_project[i] = (GameObject)Instantiate(dome_project_prefab);
      //float m = 10;
      //dome_project[i].transform.localScale = new Vector3(dome_s*m*i,dome_s*m*i,dome_s*m*i);
    }
    plane_project = new GameObject[n_projects];
    for(int i = 0; i < n_projects; i++)
    {
      plane_project[i] = (GameObject)Instantiate(plane_project_prefab);
    }

    blackhole   = GameObject.Find("BlackHole");
    galaxy      = GameObject.Find("Galaxy");
    solarsystem = GameObject.Find("SolarSystem");
    earth       = GameObject.Find("Earth");

    blackhole.GetComponent<Renderer>().material.SetColor(  "_Color",Color.blue);
    galaxy.GetComponent<Renderer>().material.SetColor(     "_Color",Color.red);
    solarsystem.GetComponent<Renderer>().material.SetColor("_Color",Color.yellow);
    earth.GetComponent<Renderer>().material.SetColor(      "_Color",Color.green);
    blackhole.GetComponent<Renderer>().material.SetColor(  "_Albedo",Color.blue);
    galaxy.GetComponent<Renderer>().material.SetColor(     "_Albedo",Color.red);
    solarsystem.GetComponent<Renderer>().material.SetColor("_Albedo",Color.yellow);
    earth.GetComponent<Renderer>().material.SetColor(      "_Albedo",Color.green);
    blackhole.GetComponent<Renderer>().material.SetColor(  "_EmissionColor",Color.blue);
    galaxy.GetComponent<Renderer>().material.SetColor(     "_EmissionColor",Color.red);
    solarsystem.GetComponent<Renderer>().material.SetColor("_EmissionColor",Color.yellow);
    earth.GetComponent<Renderer>().material.SetColor(      "_EmissionColor",Color.green);

    //zoom
    n_zooms = 4;
    zoom_cur = 0;
    zoom_next = 0;
    zoom_t = 0;

    zoom_cluster = new GameObject[n_zooms];
    zoom_cluster[0] = GameObject.Find("Zoom0Cluster");
    zoom_cluster[1] = GameObject.Find("Zoom1Cluster");
    zoom_cluster[2] = GameObject.Find("Zoom2Cluster");
    for(int i = 3; i < n_zooms; i++)
      zoom_cluster[i] = new GameObject();


    player_position_from = Vector3.zero;
    player_position_to   = Vector3.zero;

    zoom_cluster_zoom_from = new float[n_zooms];
    zoom_cluster_zoom_to = new float[n_zooms];
    zoom_cluster_zoom = new float[n_zooms,n_zooms];
    // size of earth while...
    zoom_cluster_zoom[0,0] = 1000f;  //on earth
    zoom_cluster_zoom[0,1] = 0.01f;   //on solar system
    zoom_cluster_zoom[0,2] = 0.001f;  //on galaxy
    zoom_cluster_zoom[0,3] = 0.0001f; //on beyond
    zoom_cluster_zoom_from[0] = zoom_cluster_zoom[0,0];
    zoom_cluster_zoom_to[0]   = zoom_cluster_zoom[0,0];
    // size of solar system while...
    zoom_cluster_zoom[1,0] = 10000f;  //on earth
    zoom_cluster_zoom[1,1] = 1f;    //on solar system
    zoom_cluster_zoom[1,2] = 0.1f;  //on galaxy
    zoom_cluster_zoom[1,3] = 0.01f; //beyond
    zoom_cluster_zoom_from[1] = zoom_cluster_zoom[1,0];
    zoom_cluster_zoom_to[1]   = zoom_cluster_zoom[1,0];
    // size of galaxy while...
    zoom_cluster_zoom[2,0] = 100000f; //on earth
    zoom_cluster_zoom[2,1] = 100f; //on solar system
    zoom_cluster_zoom[2,2] = 10f; //on galaxy
    zoom_cluster_zoom[2,3] = 1f; //beyond
    zoom_cluster_zoom_from[2] = zoom_cluster_zoom[2,0];
    zoom_cluster_zoom_to[2]   = zoom_cluster_zoom[2,0];
    // size of beyond while...
    zoom_cluster_zoom[3,0] = 1000000f; //on earth
    zoom_cluster_zoom[3,1] = 1000f; //on solar system
    zoom_cluster_zoom[3,2] = 100f; //on galaxy
    zoom_cluster_zoom[3,3] = 1f; //beyond
    zoom_cluster_zoom_from[3] = zoom_cluster_zoom[3,0];
    zoom_cluster_zoom_to[3]   = zoom_cluster_zoom[3,0];

    zoom_grid_resolution = new float[n_zooms];
    zoom_grid_resolution[0] = 10f;
    zoom_grid_resolution[1] = 5f;
    zoom_grid_resolution[2] = 0.5f;
    zoom_grid_resolution[3] = 0.2f;
    zoom_grid_resolution_from = zoom_grid_resolution[0];
    zoom_grid_resolution_to   = zoom_grid_resolution[0];

    zoom_grid_display_resolution = new float[n_zooms];
    zoom_grid_display_resolution[0] = 10f;
    zoom_grid_display_resolution[1] = 1f;
    zoom_grid_display_resolution[2] = 1f;
    zoom_grid_display_resolution[3] = 0.2f;
    zoom_grid_display_resolution_from = zoom_grid_display_resolution[0];
    zoom_grid_display_resolution_to   = zoom_grid_display_resolution[0];

    zoom_target_euler = new Vector2[n_zooms];
    zoom_target_inflated_euler = new Vector2[n_zooms];
    zoom_target_euler_inflation = new float[n_zooms];
    for(int i = 0; i < n_zooms; i++)
    {
      zoom_target_euler[i] = Vector2.zero;
      zoom_target_inflated_euler[i] = Vector2.zero;
    }
    zoom_target_euler_inflation[0] = 1f;
    zoom_target_euler_inflation[1] = 5f;
    zoom_target_euler_inflation[2] = 5f;
    zoom_target_euler_inflation[3] = 15f;

    blackhole_position_from = new Vector3(99999,99999,9999);
    blackhole_position_to   = new Vector3(99999,99999,9999);

    GameObject[] star_groups;
    GameObject star;
    Vector3 starpos;

    int n_stars = 5000;
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
        starpos = starpos.normalized;
        float r = Random.Range(0f,1f);
        starpos *= Mathf.Lerp(1f,5f,r*r);

        star.transform.position = starpos;
        star.transform.rotation = Quaternion.Euler(Random.Range(0f,360f),Random.Range(0f,360f),Random.Range(0f,360f));
        star.transform.localScale = new Vector3(0.005f,0.005f,0.005f);

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
    Destroy(star,0f);

    zoom_grid_resolution_cur = zoom_grid_resolution[zoom_cur];
    zoom_grid_display_resolution_cur = zoom_grid_display_resolution[zoom_cur];
    grid_alpha = 1f;
    ray_alpha = 0f;

    goal_yaw = Random.Range(-180f,180f);
    goal_pitch = Random.Range(10,60f);

    plane_collider = plane.GetComponent<Collider>();

    primaryLabel = (GameObject)Instantiate(label_prefab);
    primaryLabelText = primaryLabel.GetComponent<TextMesh>();
    earthLabel = (GameObject)Instantiate(label_prefab);
    earthLabelText = earthLabel.GetComponent<TextMesh>();
    hudLabel = (GameObject)Instantiate(label_prefab);
    hudLabelText = hudLabel.GetComponent<TextMesh>();
  }

  void Update()
  {
    if(zoom_t == 0 && Input.GetMouseButtonDown(0))
    {
      zoom_t = 0.01f;

      //find zoom next
      Vector3 view_dir = camera.transform.rotation * look_ahead;
      if(Vector3.Dot(view_dir,camera_house.transform.position.normalized) < 0) zoom_next = 0;
      else zoom_next = (zoom_next+1)%n_zooms;

      //find player_position
      player_position_from = camera_house.transform.position-player_height;
      if(zoom_next == 0)
      {
        player_position_to = new Vector3(0,0,0);
      }
      else
      {
        zoom_target_euler[zoom_cur] = snapped_lazy_origin_euler;
        player_position_to = Quaternion.Euler(-Mathf.Rad2Deg*zoom_target_euler[zoom_cur].x, -Mathf.Rad2Deg*zoom_target_euler[zoom_cur].y+90, 0) * look_ahead * Mathf.Pow(20,zoom_next);

        if(zoom_cur == 0) zoom_target_inflated_euler[zoom_cur] = zoom_target_euler[zoom_cur];
        else zoom_target_inflated_euler[zoom_cur] = zoom_target_inflated_euler[zoom_cur-1]+((zoom_target_euler[zoom_cur]-zoom_target_euler[zoom_cur-1])/zoom_target_euler_inflation[zoom_cur]);
      }

      //set lerp positions
      for(int i = 0; i < n_zooms; i++)
      {
        zoom_cluster_zoom_from[i] = zoom_cluster_zoom[i,zoom_cur];
        zoom_cluster_zoom_to[i]   = zoom_cluster_zoom[i,zoom_next];
      }
      zoom_grid_resolution_from = zoom_grid_resolution[zoom_cur];
      zoom_grid_resolution_to   = zoom_grid_resolution[zoom_next];
      zoom_grid_display_resolution_from = zoom_grid_display_resolution[zoom_cur];
      zoom_grid_display_resolution_to   = zoom_grid_display_resolution[zoom_next];

      //do special transforms
      switch(zoom_next)
      {
        case 0:
          plane.transform.position = new Vector3(0f,-9999999,0f);
          for(int i = 0; i < n_projects; i++)
            plane_project[i].transform.position = new Vector3(0f,-9999999,0f);
          blackhole_position_from = blackhole.transform.position;
          blackhole_position_to   = blackhole_position_from*10f;
          break;
        case 1:
          ground.SetActive(false);
          plane.transform.position = player_position_to + player_position_to.normalized*(dome_s*2);
          for(int i = 0; i < n_projects; i++)
            plane_project[i].transform.position = player_position_to + player_position_to.normalized*(dome_s*10*i);
          break;
        case 2:
          plane.transform.position = player_position_to + player_position_to.normalized*(dome_s*2);
          for(int i = 0; i < n_projects; i++)
            plane_project[i].transform.position = player_position_to + player_position_to.normalized*(dome_s*2*10*i);
          break;
        case 3:
          plane.transform.position = player_position_to + player_position_to.normalized*(dome_s*5);
          for(int i = 0; i < n_projects; i++)
            plane_project[i].transform.position = player_position_to + player_position_to.normalized*(dome_s*5*10*i);
          if(
            Mathf.Abs(lazy_origin_inflated_euler.x-goal_pitch) < 0.5 &&
            Mathf.Abs(lazy_origin_inflated_euler.y+goal_yaw) < 0.5
          )
          {
            blackhole_position_to   = plane.transform.position;
            blackhole_position_from = blackhole_position_to*10f;
          }
          break;
      }
      plane.transform.rotation = Quaternion.Euler(-zoom_target_euler[zoom_cur].x*Mathf.Rad2Deg+90,-zoom_target_euler[zoom_cur].y*Mathf.Rad2Deg+90,0);//+90+180,0);
      for(int i = 0; i < n_projects; i++)
        plane_project[i].transform.rotation = plane.transform.rotation;
    }

    if(zoom_t > 0)
    {
      zoom_t += 0.01f;

      if(zoom_t > 1)
      {
        zoom_t = 0;
        zoom_cur = zoom_next;
        if(zoom_next == 0)
          ground.SetActive(true);
      }
    }

    if(zoom_t > 0)
    {
      //lerp between positions
      for(int i = 0; i < n_zooms; i++)
      {
        float s = Mathf.Lerp(zoom_cluster_zoom_from[i],zoom_cluster_zoom_to[i],zoom_t);
        zoom_cluster[i].transform.localScale = new Vector3(s,s,s);
      }
      blackhole.transform.position = Vector3.Lerp(blackhole_position_from,blackhole_position_to,zoom_t);
      zoom_grid_resolution_cur         = Mathf.Lerp(zoom_grid_resolution_from,        zoom_grid_resolution_to,        zoom_t);
      zoom_grid_display_resolution_cur = Mathf.Lerp(zoom_grid_display_resolution_from,zoom_grid_display_resolution_to,zoom_t);

      //do special transforms
      if(zoom_cur == 0 && zoom_next >  0) ray_alpha =    zoom_t;
      if(zoom_cur >  0 && zoom_next == 0) ray_alpha = 1f-zoom_t;

      camera_house.transform.position = Vector3.Lerp(player_position_from, player_position_to, zoom_t) + player_height;

      grid_alpha = ((zoom_t-0.5f)*2f);
      grid_alpha = grid_alpha*grid_alpha*grid_alpha*grid_alpha;
      grid_alpha *= grid_alpha;
    }
    else
    {
      //lerp between positions
      for(int i = 0; i < n_zooms; i++)
      {
        float s = zoom_cluster_zoom_to[i];
        zoom_cluster[i].transform.localScale = new Vector3(s,s,s);
      }
      blackhole.transform.position = blackhole_position_to;
      zoom_grid_resolution_cur         = zoom_grid_resolution_to;
      zoom_grid_display_resolution_cur = zoom_grid_display_resolution_to;

      //do special transforms
      if(zoom_cur == 0) ray_alpha = 0;
      else              ray_alpha = 1;

      camera_house.transform.position = player_position_to + player_height;

      grid_alpha = 1;
    }

    camera_house.transform.rotation = Quaternion.Euler((Input.mousePosition.y-Screen.height/2)/-2, (Input.mousePosition.x-Screen.width/2)/2, 0);

    look_dir = camera.transform.rotation * look_ahead;
    if(zoom_cur == 0 && zoom_t < 0.5)
    {
      origin_to_pt = cast_vision = camera.transform.position + (look_dir * dome_s*5);;
      origin_ray = origin_to_pt.normalized;
    }
    else
    {
      float dot    = Vector3.Dot(camera.transform.position.normalized,(camera.transform.rotation * look_ahead).normalized);
      float yawdot = Vector2.Dot(new Vector2(look_dir.x,look_dir.z).normalized,new Vector2(camera.transform.position.x,camera.transform.position.z).normalized);
      dot = Mathf.Min(dot,yawdot);
      if(dot < -1f) dot = -1f;
      if(dot >= 0) cast_vision = camera.transform.position + (look_dir * dome_s*5);
      else         cast_vision = camera.transform.position + (look_dir * dome_s*5)*(1+dot);

      Ray ray = new Ray(Vector3.zero, cast_vision.normalized);
      RaycastHit hit;
      if(plane_collider.Raycast(ray, out hit, 10000.0F))
      {
        origin_to_pt = hit.point;
        origin_ray = origin_to_pt.normalized;
      }
    }

    lazy_origin_ray = Vector3.Lerp(lazy_origin_ray, origin_ray, 0.05f).normalized;

    Vector3 lazy_origin_ray_sqr = lazy_origin_ray;
    lazy_origin_ray_sqr.x *= lazy_origin_ray_sqr.x;
    lazy_origin_ray_sqr.y *= lazy_origin_ray_sqr.y;
    lazy_origin_ray_sqr.z *= lazy_origin_ray_sqr.z;
    float lazy_plane_origin_dist = Mathf.Sqrt(lazy_origin_ray_sqr.x+lazy_origin_ray_sqr.z);

    lazy_origin_euler.x = Mathf.Atan2(lazy_origin_ray.y,lazy_plane_origin_dist);
    lazy_origin_euler.y = Mathf.Atan2(lazy_origin_ray.z,lazy_origin_ray.x);
    snapped_lazy_origin_euler.x = ((Mathf.Floor((lazy_origin_euler.x*Mathf.Rad2Deg)/zoom_grid_resolution_cur)*zoom_grid_resolution_cur)+zoom_grid_resolution_cur/2)*Mathf.Deg2Rad;
    snapped_lazy_origin_euler.y = ((Mathf.Floor((lazy_origin_euler.y*Mathf.Rad2Deg)/zoom_grid_resolution_cur)*zoom_grid_resolution_cur)+zoom_grid_resolution_cur/2)*Mathf.Deg2Rad;

    snapped_lazy_origin_ray = Quaternion.Euler(-Mathf.Rad2Deg*snapped_lazy_origin_euler.x, -Mathf.Rad2Deg*snapped_lazy_origin_euler.y+90, 0) * look_ahead;

    //labels
    Vector3         lazy_gaze_position;
    Vector3 snapped_lazy_gaze_position;

    if(zoom_cur == 0 && zoom_t < 0.1)
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

    eyeray.GetComponent<LineRenderer>().SetPosition(1,lazy_gaze_position);
    eyeray.GetComponent<LineRenderer>().SetPosition(2,lazy_gaze_position*100);

    primaryLabel.transform.position = lazy_gaze_position+new Vector3(0f,0.5f,0f);
    primaryLabel.transform.rotation =  Quaternion.Euler(-        lazy_origin_euler.x*Mathf.Rad2Deg,90f-        lazy_origin_euler.y*Mathf.Rad2Deg,0);

    hudLabel.transform.position = camera.transform.position + (camera.transform.rotation * look_ahead * dome_s);
    hudLabel.transform.rotation = camera.transform.rotation;
    hudLabelText.text = string.Format("\n\n\n\n\n\nHud {0}° {1}°",goal_yaw,goal_pitch);

    if(zoom_cur != 0)
    {
      earthLabel.transform.position = camera_house.transform.position.normalized * (camera_house.transform.position.magnitude-dome_s);
      earthLabel.transform.rotation = Quaternion.Euler(lazy_origin_euler.x*Mathf.Rad2Deg,270f-lazy_origin_euler.y*Mathf.Rad2Deg,0);
      if(zoom_cur == 1)
        earthLabelText.text = string.Format("Solar System\n{0} mi",camera_house.transform.position.magnitude*camera_house.transform.position.magnitude);
      else if(zoom_cur == 2)
        earthLabelText.text = string.Format("Milky Way\n{0} mi",camera_house.transform.position.magnitude*camera_house.transform.position.magnitude);
      else if(zoom_cur == 3)
        earthLabelText.text = string.Format("Home\n{0} mi",camera_house.transform.position.magnitude*camera_house.transform.position.magnitude);
    }
    else
      earthLabelText.text = "";

    lazy_origin_inflated_euler = lazy_origin_euler;
    if(zoom_cur != 0) lazy_origin_inflated_euler = zoom_target_inflated_euler[zoom_cur-1]+((lazy_origin_euler-zoom_target_euler[zoom_cur-1])/zoom_target_euler_inflation[zoom_cur]);
    lazy_origin_inflated_euler *= Mathf.Rad2Deg;

    float lookx = lazy_origin_inflated_euler.x;
    float looky = -lazy_origin_inflated_euler.y;
    float looky_min = Mathf.Floor(looky/zoom_grid_display_resolution_cur)*zoom_grid_display_resolution_cur;
    float looky_max = Mathf.Ceil(looky/zoom_grid_display_resolution_cur)*zoom_grid_display_resolution_cur;
    float lookx_min = Mathf.Floor(lookx/zoom_grid_display_resolution_cur)*zoom_grid_display_resolution_cur;
    float lookx_max = Mathf.Ceil(lookx/zoom_grid_display_resolution_cur)*zoom_grid_display_resolution_cur;
    if(zoom_cur < 2)
      primaryLabelText.text = string.Format("{0}° - {1}°,\n{2}° - {3}°", looky_min.ToString("F0"), looky_max.ToString("F0"), lookx_min.ToString("F0"), lookx_max.ToString("F0"));
    else if(zoom_cur < 3)
      primaryLabelText.text = string.Format("{0}°°,\n{1}°", looky.ToString("F2"), lookx.ToString("F2"));
    else
      primaryLabelText.text = "";

    //shader inputs
    grid_material.SetVector(camera_position_id,camera.transform.position);
    grid_material.SetVector(lazy_origin_ray_id,lazy_origin_ray);
    grid_material.SetFloat(snapped_lazy_origin_pitch_id,snapped_lazy_origin_euler.x);
    grid_material.SetFloat(snapped_lazy_origin_yaw_id,snapped_lazy_origin_euler.y);
    grid_material.SetFloat(grid_resolution_id,zoom_grid_resolution_cur);
    grid_material.SetFloat(grid_alpha_id,grid_alpha);

    project_grid_material.SetVector(camera_position_id,camera.transform.position);
    project_grid_material.SetVector(lazy_origin_ray_id,lazy_origin_ray);
    project_grid_material.SetFloat(snapped_lazy_origin_pitch_id,snapped_lazy_origin_euler.x);
    project_grid_material.SetFloat(snapped_lazy_origin_yaw_id,snapped_lazy_origin_euler.y);
    project_grid_material.SetFloat(grid_resolution_id,zoom_grid_resolution_cur);
    project_grid_material.SetFloat(grid_alpha_id,grid_alpha);

    ray_material.SetVector(camera_position_id,camera.transform.position);
    ray_material.SetFloat(ray_alpha_id,ray_alpha);
  }

}

