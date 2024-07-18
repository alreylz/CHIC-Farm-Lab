using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))][RequireComponent(typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{

    Mesh mesh;
    //Componente que se encarga de definir la forma de el elemento 3D asociado a este GameObject.
    MeshFilter meshFilter;
    //Componente que controla cómo se renderiza el objeto 3d, cómo se visualiza.
    MeshRenderer meshRenderer;

    public Material material;

    Vector3 [] vecArrayOfPoints;


    public Transform p0, p1, p2, p3;

    public int[] triangles;
         

    // Start is called before the first frame update
    void Start()
    {
        //inicializamos los atributos de nuestra clase, guardando referencias a componentes de nuestro GameObject 
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        //Creamos una nueva malla cuya forma definiremos con vectores posteriormente
        mesh = new Mesh();
        mesh.name = "PG_Mesh";



        // global coordinates - offset introduced by the gameobject itself
        vecArrayOfPoints = new Vector3[] { p0.position - transform.position, p1.position- transform.position, p2.position- transform.position, p3.position- transform.position };
        //Especificamos el orden de los puntos para la creación de triángulos.

        triangles = new int[] {
            0, 1, 2,
            1, 3, 2
        };



        mesh.vertices = vecArrayOfPoints;
        mesh.triangles = triangles;


        meshRenderer.material = material;
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
    }





}
