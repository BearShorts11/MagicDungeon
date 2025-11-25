using UnityEngine;

//ISpell exists to force all spells to check SpellContext for relevant data
//Abstract class to force all derived spells to define methods using data from ISpell
public abstract class SpellLogic: ScriptableObject, ISpell
{
    public abstract bool CanCast(SpellContext context);
    public abstract void Cast(SpellContext context);
}
