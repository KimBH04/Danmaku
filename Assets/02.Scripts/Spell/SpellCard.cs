using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/SpellPattern")]
public class SpellCard : ScriptableObject
{
    [field: SerializeField, FormerlySerializedAs("spellName")]
    public string SpellName { get; private set; }

    [field: SerializeField, TextArea, FormerlySerializedAs("spellDescription")]
    public string SpellDescription { get; private set; }

    [field: SerializeField, FormerlySerializedAs("presentHP")]
    public int PresentHP { get; private set; }

    [field: SerializeField, FormerlySerializedAs("timeLimit")]
    public int TimeLimit { get; private set; }

    [SerializeField] private BurstData[] bursts;

    public IEnumerable<BurstData> Bursts => bursts;
}
