using System.Collections;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    public SpellCard[] spellCards;

    public void SpellOneShot()
    {
        foreach (var spell in spellCards)
        {
            BulletManager.Instance.BurstChain(spell.Bursts);
        }
    }
}