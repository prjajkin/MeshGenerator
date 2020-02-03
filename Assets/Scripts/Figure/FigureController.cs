using UnityEngine;

namespace Assets.Scripts.Figure
{
    public class FigureController : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private RotateController rotateController;

        public MeshFilter MeshFilter => meshFilter;

        public void Init(Mesh mesh)
        {
            meshFilter.mesh.Clear();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = meshFilter.mesh;
            Vector2[] uvs = new Vector2[mesh.vertices.Length];

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
            }
            meshFilter.mesh.uv = uvs;
        }


        public void SetRotate(bool value)
        {
            rotateController.SetRotate(value);
        } 

        public void UpdateMeshRenderer()
        {
            meshCollider.sharedMesh = meshFilter.mesh; 
        }

        private void OnDestroy()
        {
            Destroy(meshFilter);
            Destroy(gameObject);
        }
         
    }
}
