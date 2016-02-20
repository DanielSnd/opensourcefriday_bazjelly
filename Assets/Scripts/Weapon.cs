using UnityEngine;
using System.Collections;
using System.Linq;

public class Weapon : CacheBehaviour
{
    public ConstantForce2D cF;
    
    void OnEnable()
    {
        SetWind(GameManager.get.wind);
    }

    public void SetWind(float windpower)
    {
        if (!cF) return;
        cF.force = new Vector2(windpower, 0);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject != gameObject)
        {
            Explode();
        }
    }

    public void Explode()
    {
        var playersInRange = Physics2D.OverlapCircleAll(transform.position, 1);

        foreach (Collider2D _d in playersInRange)
        {
            Actor getActor = _d.GetComponent<Actor>();
            if (getActor != null)
            {
                getActor.TakeDamage(10);
            }
        }
        
        this.Recycle();
    }

}
