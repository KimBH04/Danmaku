using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/SpellPattern")]
public class SpellCard : ScriptableObject
{
    [SerializeField] private string spellName;
    [SerializeField, TextArea] private string spellDescription;
    [SerializeField] private BurstData[] bursts;

    public string SpellName => spellName;
    public string SpellDescription => spellDescription;
    public IEnumerable<BurstData> Bursts => bursts;
}