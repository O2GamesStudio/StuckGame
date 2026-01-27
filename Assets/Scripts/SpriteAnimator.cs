using UnityEngine;
using Lean.Pool;

public class SpriteAnimator : MonoBehaviour, IPoolable
{
    [SerializeField] Sprite[] sprites;


    void PlayAnimation()
    {

    }
    public void OnDespawn()
    {
        throw new System.NotImplementedException();
    }

    public void OnSpawn()
    {
        throw new System.NotImplementedException();
    }
}
