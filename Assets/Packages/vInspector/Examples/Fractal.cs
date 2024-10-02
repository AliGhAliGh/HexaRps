#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

using VInspector;


namespace VInspectorExamples
{
    [ExecuteInEditMode]
    public class Fractal : MonoBehaviour
    {

        [Tab("Shape")]

        [Variants("Sponge", "Cloud", "Snowflake")]
        public string type;
        [RangeResettable(1, 4)]
        public int steps = 3;

        [Space(4)]
        [Foldout("Modifications")]
        [RangeResettable(0, 1)]
        public float gaps = 0;
        [RangeResettable(0, 1)]
        public float jitter = 0;




        [Tab("Animation")]

        [RangeResettable(0, 1)]
        public float speed = .25f;
        [RangeResettable(0, 1)]
        public float direction = 0;

        [Button]
        [Tab("Animation")]
        public bool playAnimation = false;




        [Tab("Rendering")]

        public Mesh mesh;
        public Material material;
        public bool shadows = true;



        [Tab("Settings")]

        public SerializedDictionary<string, Color> someColorDictionary;





        [Tab("Shape")]
        [Button]
        private void Generate()
        {
            Delete();
            GeneratePrimitive();
            GeneratePositions();
            GenerateStep(transform);
            DestroyImmediate(primitive);
        }


        [Tab("Shape")]
        [Button]
        [ButtonSize(22)]
        private void Delete()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }




        [Tab("Rendering")]
        [Button]
        private void ApplyRenderingSettings()
        {
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = material;
                meshRenderer.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.GetComponent<MeshFilter>().mesh = mesh;
            }
        }







        // add this variable to your script so vInspector won't forget after recompilation which foldouts are expanded and which tabs are selected
        public VInspectorData vInspectorData;


        private void GeneratePrimitive()
        {
            primitive = new GameObject();
            primitive.AddComponent<MeshFilter>().mesh = mesh;
            primitive.AddComponent<MeshRenderer>().material = material;
            primitive.GetComponent<MeshRenderer>().shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

        }

        private void GeneratePositions()
        {
            positions = new List<Vector3>();
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 1; y++)
                    for (var z = -1; z <= 1; z++)
                        if (
                        (type == "Sponge" && x * y + y * z + z * x != 0) ||
                        (type == "Cloud" && x == 0 ^ y == 0 ^ z == 0 && (x != 0 || y != 0 || z != 0)) ||
                        (type == "Snowflake" && x + y == 0 ^ y + z == 0 ^ z + x == 0))
                            positions.Add(new Vector3(x, y, z));
        }

        private void GenerateStep(Transform parent, int step = 1)
        {
            foreach (var position in positions)
            {
                var child = (step == steps ? Instantiate(primitive) : new GameObject()).transform;

                child.parent = parent;
                child.localScale = Vector3.one / 3;
                child.localPosition = position / 3;

                child.localScale -= Vector3.one * gaps / 20;
                child.position += (new Vector3(Random.value, Random.value, Random.value) - Vector3.one / 2) * jitter;

                if (step != steps)
                    GenerateStep(child, step + 1);
            }

        }

        private GameObject primitive;
        private List<Vector3> positions;


        private void PlayAnimation(Transform parent)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                var dir = new Vector3(child.localPosition.y, child.localPosition.z, child.localPosition.x);
                dir = Vector3.Lerp(dir, child.localPosition, direction);

                child.Rotate(dir, speed * 2, Space.Self);


                PlayAnimation(child);
            }
        }

        private void OnDrawGizmos()
        {
            if (playAnimation)
                PlayAnimation(transform);

            UnityEditor.SceneView.RepaintAll();
        }

        private void Update() { }

    }
}
#endif