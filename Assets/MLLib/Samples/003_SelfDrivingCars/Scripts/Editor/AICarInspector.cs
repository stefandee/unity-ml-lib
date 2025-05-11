namespace PironGames.MLLib.Samples.SelfDrivingCars.Editor
{
    using PironGames.MLLib.Samples.Common;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public enum VelocityUnit
    {
        MPS,
        KMH,
        MPH
    }

    public class AICarInspector : EditorWindow
    {
        private const int RAYCAST_LAYER_MASK = 1 << 3;

        private const string UI_ASSETS_BASE_PATH = "Assets/MLLib/Samples/003_SelfDrivingCars/Prefabs/UIToolkit";

        private Label m_carName;

        private DropdownField m_velocityUnit;
        private TextField m_carVelocity;
        private Label m_nextWP;
        private Button m_btnCameraFollow;

        private ListView m_lapList;
        private TextField m_averageLap;
        private TextField m_currentLap;

        private Foldout m_basicFoldout;
        private Foldout m_lapsFoldout;
        private Foldout m_nnFoldout;
        private NNGraphView m_nnGraph;

        //private DropdownField _primitiveType;
        //private Toggle _isActiveToggle;
        //private Button _createButton;

        private GameObject m_selected = null;
        private IRaceRecorderEntity m_recorder = null;

        private VisualTreeAsset m_lapStatsAsset;

        [MenuItem("Tools/Piron Games/MLLib/Self Driving Cars/Car Inspector")]
        public static void ShowWindow()
        {
            AICarInspector wnd = GetWindow<AICarInspector>();
            wnd.titleContent = new GUIContent("Car Inspector");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UI_ASSETS_BASE_PATH}/CarInspector.uxml");
            VisualElement doc = visualTree.Instantiate();
            root.Add(doc);

            m_lapStatsAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UI_ASSETS_BASE_PATH}/LapStatsTemplate.uxml");

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UI_ASSETS_BASE_PATH}/CarInspector.uss");
            //root.styleSheets.Add(styleSheet);

            // basic stats foldout
            m_basicFoldout = rootVisualElement.Query<Foldout>("basicFoldout");
            m_carName = rootVisualElement.Query<Label>("carName");

            m_carVelocity = rootVisualElement.Query<TextField>("velocity");
            m_velocityUnit = rootVisualElement.Query<DropdownField>("velocityUnit").First();

            m_nextWP = rootVisualElement.Query<Label>("nextwp");

            m_btnCameraFollow = rootVisualElement.Query<Button>("btnCameraFollow");
            m_btnCameraFollow.RegisterCallback<ClickEvent>(OnBtnCameraFollowClicked);

            // laps foldout
            m_lapsFoldout = rootVisualElement.Query<Foldout>("lapsFoldout");

            m_lapList = rootVisualElement.Query<ListView>("laplist").First();

            m_averageLap = rootVisualElement.Query<TextField>("averageLap");
            m_currentLap = rootVisualElement.Query<TextField>("currentLap");

            // neural network foldout
            m_nnFoldout = rootVisualElement.Query<Foldout>("nnFoldout");

            //if (m_nnGraph == null)
            //{
            //    m_nnGraph = new();
            //    m_nnGraph.StretchToParentSize();
            //    m_nnFoldout.Add(m_nnGraph);

            //    var node = new Node();
            //    node.title = "Node666";
            //    m_nnGraph.AddElement(node);
            //}

            // Getting the UI element by name needs type cast
            // _primitiveType = (DropdownField)rootVisualElement.Query("primitiveSelector").First();

            // Getting the UI element by type and name
            //_isActiveToggle = rootVisualElement.Query<Toggle>("IsActive");
            // _createButton = rootVisualElement.Query<Button>("create");
            // _createButton.clicked += CreatePrimitive;

            ResetUI();
        }

        private void OnEnable()
        {
        }
        private void CreatePrimitive()
        {
            //var primitiveType = Enum.Parse<PrimitiveType>(_primitiveType.choices[_primitiveType.index], true);
            //var created = GameObject.CreatePrimitive(primitiveType);
            //created.transform.position = Vector3.zero;
            //created.name = _textField == null || _textField.text == "" ? primitiveType.ToString() : _textField.text;
            //created.SetActive(true);
        }

        void Update()
        {
            bool reset = false;

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit raycastHit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out raycastHit, 500f, RAYCAST_LAYER_MASK) && raycastHit.transform != null)
                    {
                        CleanupRecorderEvents();

                        m_selected = raycastHit.transform.gameObject;

                        m_recorder = m_selected != null ? m_selected.GetComponent<IRaceRecorderEntity>() : null;

                        if (m_recorder != null)
                        {
                            m_recorder.RaceRecorder.OnCurrentLapTimeChanged += OnCurrentLapTimeChanged;
                            m_recorder.RaceRecorder.OnAverageLapTimeChanged += OnAverageLapTimeChanged;
                            m_recorder.RaceRecorder.OnStatsChanged += OnStatsChanged;

                            OnAverageLapTimeChanged(m_recorder.RaceRecorder.AverageLapTime);
                            OnStatsChanged(m_recorder.RaceRecorder.GetBestLaps());
                        }
                    }
                    else
                    {
                        Cleanup();
                        reset = true;
                    }
                }

                if (m_selected != null)
                {
                    UpdateUI();
                    Repaint();
                }
            }
            else
            {
                Cleanup();
                reset = true;
            }

            if (reset)
            {
                ResetUI();
                Repaint();
            }
        }

        void OnDestroy()
        {
            CleanupRecorderEvents();
            ResetUI();
            Debug.Log("Car Inspector Destroyed...");
        }

        void ResetUI()
        {
            m_carName.text = "No Car Selected";
            m_carVelocity.value = "N/A";
            m_averageLap.value = "N/A";
            m_currentLap.value = "N/A";
            m_nextWP.text = "Next Waypoint: N/A";

            m_lapList.itemsSource = new List<LapStats>();
            m_lapList.RefreshItems();
        }

        void CleanupRecorderEvents()
        {
            if (m_recorder != null)
            {
                m_recorder.RaceRecorder.OnCurrentLapTimeChanged -= OnCurrentLapTimeChanged;
                m_recorder.RaceRecorder.OnAverageLapTimeChanged -= OnAverageLapTimeChanged;
                m_recorder.RaceRecorder.OnStatsChanged -= OnStatsChanged;
            }
        }

        void Cleanup()
        {
            CleanupRecorderEvents();
            m_selected = null;
        }

        void UpdateUI()
        {
            IAICar aiCar = m_selected != null ? m_selected.GetComponent<IAICar>() : null;

            if (aiCar == null)
            {
                ResetUI();
                return;
            }

            var rb = m_selected.GetComponentInChildren<Rigidbody>();

            m_carName.text = m_selected.name;

            if (aiCar is IWaypointFollower wpFollower)
            {
                var wpName = wpFollower.Waypoint != null && wpFollower.Waypoint.Next != null ? wpFollower.Waypoint.Next.name : "N/A";
                m_nextWP.text = $"Next Waypoint: {wpName}";
            }
            else
            {
                m_nextWP.text = "Next Waypoint: N/A";
            }

            var velocityUnit = Enum.Parse<VelocityUnit>(m_velocityUnit.choices[m_velocityUnit.index], true);
            var displayVelocity = rb.linearVelocity.magnitude * VelocityConversionFactor(velocityUnit);

            m_carVelocity.value = rb != null ? displayVelocity.ToString("n2") : "N/A";

            UpdateNN(aiCar);
        }

        void UpdateNN(IAICar aiCar)
        {

        }

        void OnCurrentLapTimeChanged(float value)
        {
            if (m_recorder != null)
            {
                m_currentLap.value = FormatLapTime(value);
            }
        }

        void OnAverageLapTimeChanged(float value)
        {
            if (m_recorder != null)
            {
                m_averageLap.value = !float.IsNaN(value) ? FormatLapTime(value) : "N/A";
            }
        }

        void OnStatsChanged(List<LapStats> list)
        {
            // The "makeItem" function will be called as needed
            // when the ListView needs more items to render
            Func<VisualElement> makeItem = () => m_lapStatsAsset.Instantiate();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (i >= list.Count)
                {
                    return;
                }

                var stat = list[i];

                var lap = e.Query<Label>("lap").First();
                lap.text = $"Lap {stat.Lap}";

                var collisions = e.Query<Label>("collisions").First();
                collisions.text = $"Collisions {stat.Crashes}";

                var time = e.Query<TextField>("time").First();
                time.value = FormatLapTime(stat.Time);
            };

            m_lapList.makeItem = makeItem;
            m_lapList.bindItem = bindItem;
            m_lapList.itemsSource = list;

            m_lapList.RefreshItems();

            //foreach (var stat in list)
            //{
            //    VisualElement listEntry = m_lapStatsAsset.Instantiate();

            //    var lap = listEntry.Query<Label>("lap").First();
            //    lap.text = $"Lap {stat.Lap}";

            //    var collisions = listEntry.Query<Label>("collisions").First();
            //    collisions.text = $"Collisions {stat.Crashes}";

            //    var time = listEntry.Query<TextField>("time").First();
            //    time.value = FormatLapTime(stat.Time);

            //    m_lapList.Add(listEntry);
            //}
        }

        private void OnBtnCameraFollowClicked(ClickEvent evt)
        {
            if (!m_selected)
            {
                return;
            }

            var cameraFollow = Camera.main.GetComponent<CameraFollow>();

            if (cameraFollow)
            {
                cameraFollow.Target = m_selected.transform;
            }
        }

        float VelocityConversionFactor(VelocityUnit unit)
        {
            return unit switch
            {
                VelocityUnit.KMH => 3.6f,
                VelocityUnit.MPH => 2.23694f,
                _ => 1.0f,
            };
        }

        string FormatLapTime(float time)
        {
            int s = Mathf.FloorToInt(time);
            int ms = (int)((time - s) * 10);

            var h = s / 3600;
            var m = s / 60;
            var ss = s - h * 3600 - m * 60;

            if (h > 0)
            {
                return string.Format("{0:00}:{1:00}:{2:00}", h, m, ss);
            }
            else
            {
                return string.Format("{0:00}:{1:00}:{2:00}", m, ss, ms);
            }
        }
    }
}