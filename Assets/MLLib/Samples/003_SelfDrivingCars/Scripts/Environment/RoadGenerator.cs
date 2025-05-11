namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using UnityEngine;

    namespace AICar
    {
        public class RoadGenerator : MonoBehaviour
        {
            public GameObject RoadTilePrefab;

            public GameObject RoadSidePropPrefab;

            public GameObject TrafficCar;

            public Vector3 Offset;

            public Vector3 Scale;

            public int InitialAmount = 20;

            // Use this for initialization
            void Start()
            {
                float pos = 0;

                for (int i = 0; i < InitialAmount; i++)
                {
                    GameObject tile = Instantiate(RoadTilePrefab, transform);
                    tile.name = $"Road Tile {i}";
                    tile.transform.localScale = Scale;
                    tile.transform.localPosition = Offset + new Vector3(0, 0, pos);
                    tile.transform.localRotation = Quaternion.Euler(0, 90, 0);

                    var tileExtents = tile.GetComponentInChildren<MeshRenderer>().bounds.extents;

                    if (RoadSidePropPrefab != null)
                    {
                        GameObject prop = Instantiate(RoadSidePropPrefab, transform);
                        prop.name = $"Prop {i}";
                        prop.transform.localScale = Scale / 2;
                        prop.transform.localPosition = Offset + new Vector3(tileExtents.x - 0.25f, 0, pos);
                        prop.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    }

                    pos += 2 * tileExtents.z;

                    // generate traffic
                    if (Random.Range(0, 1f) < 0.75f)
                    {
                        // multiple traffic cars per tile
                        var trafficX = -tileExtents.x * 0.75f + 0.8f;

                        while (trafficX < tileExtents.x * 0.75f)
                        {
                            if (Random.Range(0, 1f) < 0.3f)
                            {
                                GameObject traffic = Instantiate(TrafficCar, transform);
                                var trafficExtents = traffic.GetComponentInChildren<MeshRenderer>().bounds.extents;

                                traffic.transform.localPosition = new Vector3(trafficX, 0, pos + Random.Range(-tileExtents.z * 0.75f, tileExtents.z * 0.75f));

                                TrafficCarBehaviour b = traffic.GetComponent<TrafficCarBehaviour>();

                                if (b != null)
                                {
                                    b.SetRandomVelocity(2.5f, 5f);
                                }
                            }

                            trafficX += 0.8f + Random.Range(0.8f * 0.25f, 0.8f);
                        }
                    }
                }
            }

            // Update is called once per frame
            void Update()
            {

            }

            void GenerateTraffic()
            {

            }
        }
    }
}