using UnityEngine;

public interface ISpell
{
    bool CanCast(SpellContext context);
    void Cast(SpellContext context);
}
