using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/SpellPattern")]
public class SpellCard : ScriptableObject
{
    [SerializeField] private string spellName;
    [SerializeField, TextArea] private string spellDescription;
    [SerializeField] private BulletPattern[] bulletPatterns;

    public string SpellName => spellName;
    public string SpellDescription => spellDescription;
    public IEnumerable<BulletPattern> BulletPatterns => bulletPatterns;
}