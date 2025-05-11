namespace PironGames.MLLib.Samples.QLearning.Editor
{
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public class GenerateGridFromPrefab : EditorWindow
    {
        private Vector3Int m_Amount = new(3, 3, 3);
        private Vector3 m_Spacing = new(1, 1, 1);
        private Vector3 m_Center = new(0, 0, 0);
        private GameObject m_Source;

        [MenuItem("Tools/Piron Games/MLLib/Q Learning/Generate Grid From Prefab")]
        public static void ShowWindow()
        {
            var window = GetWindow<GenerateGridFromPrefab>();
            window.titleContent = new GUIContent("Generate Grid From Prefab");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            m_Amount = EditorGUILayout.Vector3IntField("Amount", m_Amount);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            m_Center = EditorGUILayout.Vector3Field("Center", m_Center);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            m_Spacing = EditorGUILayout.Vector3Field("Spacing", m_Spacing);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            m_Source = EditorGUILayout.ObjectField("Prefab", m_Source, typeof(GameObject), false) as GameObject;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);

            EditorGUI.BeginDisabledGroup(m_Source == null || !IsAmountValid());
            if (GUILayout.Button("Generate"))
            {
                GenerateGrid();
            }
            EditorGUI.EndDisabledGroup();
        }

        private bool IsAmountValid()
        {
            return m_Amount.x > 0 && m_Amount.y > 0 && m_Amount.z > 0;
        }

        private void GenerateGrid()
        {
            if (m_Source == null)
            {
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();

            Vector3 start = m_Center + new Vector3(-(m_Amount.x - 1) * m_Spacing.x / 2, -(m_Amount.y - 1) * m_Spacing.y / 2, -(m_Amount.z - 1) * m_Spacing.z / 2);
            Vector3 offset = new();

            int instanceIndex = 0;

            int undoId = Undo.GetCurrentGroup();

            GameObject[,,] generated = new GameObject[m_Amount.x, m_Amount.y, m_Amount.z];

            for (int x = 0; x < m_Amount.x; x++)
            {
                offset.y = 0;

                for (int y = 0; y < m_Amount.y; y++)
                {

                    offset.z = 0;

                    for (int z = 0; z < m_Amount.z; z++)
                    {
                        GameObject instance = PrefabUtility.InstantiatePrefab(m_Source) as GameObject;
                        //GameObject instance = Instantiate(m_Source);

                        //PrefabUtility.ConnectGameObjectToPrefab(instance, PrefabUtility.GetPrefabParent(instance) as GameObject);

                        string instanceName = $"{m_Source.name} {instanceIndex++}";
                        instance.name = instanceName;

                        instance.transform.position = start + offset;

                        Undo.RegisterCreatedObjectUndo(instance, $"Grid of {m_Source.name}");
                        Undo.CollapseUndoOperations(undoId);

                        offset.z += m_Spacing.z;

                        generated[x, y, z] = instance;
                    }

                    offset.y += m_Spacing.y;
                }

                offset.x += m_Spacing.x;
            }

            ConnectTileInternals(generated, m_Amount);

            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        // TODO take into account size and generate 2D or 3D grid
        private void ConnectTileInternals(GameObject[,,] generated, Vector3Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        StateTileNWay tile = generated[x, y, z].GetComponentInChildren<StateTileNWay>();

                        if (tile == null)
                        {
                            continue;
                        }

                        // TODO replace neighbors index with DirectionActionIndex
                        tile.Neighbors[2].State = (x == 0) ? null : generated[x - 1, y, z];
                        tile.Neighbors[3].State = (x == size.x - 1) ? null : generated[x + 1, y, z];

                        tile.Neighbors[0].State = (z == 0) ? null : generated[x, y, z - 1];
                        tile.Neighbors[1].State = (z == size.z - 1) ? null : generated[x, y, z + 1];
                    }
                }
            }
        }
    }
}