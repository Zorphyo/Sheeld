using UnityEngine;
using Core.Interfaces;

[RequireComponent(typeof(SphereCollider))]

public class Throwable : MonoBehaviour, IInteractable
{
    SphereCollider collider;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
    }

    public void Interact()
    {
        Debug.Log("Good");
    }
}