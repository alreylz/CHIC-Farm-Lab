using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
/*Componente que permite mostrar la trayectoria de un objeto si se deja en caída libre */
public class WorldFallPreview : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Material lineMaterial; // Optional
    public GameObject onCollissionPointObject; // Optional
    public LayerMask collisionIgnoreLayer; /* Permite especificar con qué elementos va a poder chocar el rayo de previsualización de la caída */
    public Vector3 rotation;
    [Range(0.1f,2f)]
    public float updatePeriod;

    [Range(0.001f, 2f)]
    public float lineWidth0;
    [Range(0.001f, 2f)]
    public float lineWidthN;

    [Range(0.1f,6f)]
    public float scaleCollisionPointElem;


    private GameObject _CollisionObjectInstance;



    public bool AutoShow = false; // Depende de Rigidbody
    [SerializeField]
    Rigidbody fallPreviewRb;

    private void OnEnable()
    {
        if (_CollisionObjectInstance == null && onCollissionPointObject != null)
        {
            _CollisionObjectInstance = Instantiate(onCollissionPointObject);
        }
        if(lineRenderer == null) lineRenderer = gameObject.GetComponent<LineRenderer>();
        
       



        if (AutoShow && fallPreviewRb!=null)
        {
            StartCoroutine(ShowFallPreviewAuto());
        }
        else
        {
            StartCoroutine(ShowFallPreview());
            _CollisionObjectInstance.SetActive(true);
            lineRenderer.enabled = true;
        }
        

    }
    IEnumerator ShowFallPreviewAuto()
    {
        while (AutoShow)
        {
            while (fallPreviewRb.useGravity)
            {
                _CollisionObjectInstance.SetActive(true);
                lineRenderer.enabled = true;
                _CollisionObjectInstance.gameObject.transform.localScale = Vector3.one * scaleCollisionPointElem;

                RaycastHit rayHit;
                Vector3 from = gameObject.transform.position;
                Vector3 to;
                /*Lanzamos raycast al hacia el suelo para ver dónde impacta y obtener "to" (el punto de impacto)*/
                if (!Physics.Raycast(from, Vector3.down, out rayHit, 15f, ~collisionIgnoreLayer, QueryTriggerInteraction.Ignore))
                {
                    _CollisionObjectInstance.SetActive(false);
                }
                to = rayHit.point;


                Vector3 v1 = (from - to); // Vector hacia arriba hasta punto 
                Vector3 normalToSurface = rayHit.normal;

                /* Mostramos el elemento en el punto de impacto */
                _CollisionObjectInstance.transform.position = to;
                _CollisionObjectInstance.SetActive(true);

                lineRenderer.SetPositions(new Vector3[] { from, to });
                lineRenderer.startWidth = lineWidth0;
                lineRenderer.endWidth = lineWidthN;
                if (lineMaterial != null)
                {
                    lineRenderer.material = lineMaterial;
                }
                lineRenderer.useWorldSpace = true;
                yield return new WaitForSeconds(updatePeriod);
            }

            _CollisionObjectInstance.SetActive(false);
            lineRenderer.enabled = false;
            yield return new WaitUntil(() => AutoShow);
        }
    }


    IEnumerator ShowFallPreview()
    {
        while (gameObject.activeInHierarchy == true)
        {
            _CollisionObjectInstance.gameObject.transform.localScale = Vector3.one * scaleCollisionPointElem;

            RaycastHit rayHit;
            Vector3 from = gameObject.transform.position;
            Vector3 to;
            /*Lanzamos raycast al hacia el suelo para ver dónde impacta y obtener "to" (el punto de impacto)*/
            if (!Physics.Raycast(from, Vector3.down, out rayHit, 15f, ~collisionIgnoreLayer, QueryTriggerInteraction.Ignore))
            {
                _CollisionObjectInstance.SetActive(false);
            }
            to = rayHit.point;


            Vector3 v1 = (from - to); // Vector hacia arriba hasta punto 
            Vector3 normalToSurface = rayHit.normal;

            /* Mostramos el elemento en el punto de impacto */
            _CollisionObjectInstance.transform.position = to;
            _CollisionObjectInstance.SetActive(true);

            lineRenderer.SetPositions(new Vector3[] { from, to });
            lineRenderer.startWidth = lineWidth0;
            lineRenderer.endWidth = lineWidthN;
            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
            lineRenderer.useWorldSpace = true;
            yield return new WaitForSeconds(updatePeriod);
        }
    }
    private void OnDisable()
    {
        if(_CollisionObjectInstance !=null)  _CollisionObjectInstance.SetActive(false);
        if(lineRenderer != null) lineRenderer.enabled = false;
        StopAllCoroutines();
    }

}
