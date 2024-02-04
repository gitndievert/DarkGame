using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ExitPoint : MonoBehaviour
{
    public int NextCampaignId;
    public int NextMapId;

    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != Tags.PLAYER_TAG) return;
        SceneSwapper.Instance.StartScene(NextCampaignId, NextMapId);
    }
    
}
