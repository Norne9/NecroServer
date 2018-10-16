using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Game
{
    public class Effect
    {
        public int EffectId { get; set; }
        public float EffectTime { get; }
        public DateTime EndTime { get; }
        public UnitStats StatsChange { get; }
        public VisualEffect VisualEffect { get; }
        public bool RemoveByDamage { get; }
        public bool RemoveByAttack { get; }

        public Effect(int id, UnitStats stats, float time, VisualEffect effect, bool dmgRemove, bool atcRemove)
        {
            EffectId = id;
            StatsChange = stats;
            EffectTime = time;
            EndTime = DateTime.Now.AddSeconds(time);
            VisualEffect = effect;
            RemoveByDamage = dmgRemove;
            RemoveByAttack = atcRemove;
        }

        public static Effect DoubleDamage()
        {
            var ddStats = UnitStats.GetDefaultEffect();
            ddStats.Damage = 2f;
            return new Effect(0, ddStats, 20f, VisualEffect.Damage, false, false);
        }

        public static Effect Haste()
        {
            var hasteStats = UnitStats.GetDefaultEffect();
            hasteStats.MoveSpeed = 2f;
            return new Effect(1, hasteStats, 20f, VisualEffect.Haste, false, false);
        }

        public static Effect Stealth()
        {
            var stealthStats = UnitStats.GetDefaultEffect();
            stealthStats.UnitVisible = false;
            return new Effect(2, stealthStats, 20f, VisualEffect.Stealth, false, true);
        }

        public static Effect Neutrall()
        {
            var neutrallStats = UnitStats.GetDefaultEffect();
            neutrallStats.ViewRadius = 3f;
            neutrallStats.TakeDamageMultiplier = 0.4f;
            neutrallStats.Damage = 2f;
            neutrallStats.MoveSpeed = 0.5f;
            return new Effect(3, neutrallStats, 1f, VisualEffect.None, false, false);
        }

        public static void AddEffect(List<Effect> effects, Effect effect)
        {
            var same = effects.Where((e) => e.EffectId == effect.EffectId).FirstOrDefault();
            if (same != null) effects.Remove(same);
            effects.Add(effect);
        }

        public static void ProcessEffects(List<Effect> effects) =>
            effects.RemoveAll((e) => DateTime.Now > e.EndTime);

        public static VisualEffect GetVisual(List<Effect> effects)
        {
            if (effects.Count == 0)
                return VisualEffect.None;
            else
                return effects.First().VisualEffect;
        }

        public static void RemoveDamage(List<Effect> effects) =>
            effects.RemoveAll((e) => e.RemoveByDamage);

        public static void RemoveAttack(List<Effect> effects) =>
            effects.RemoveAll((e) => e.RemoveByAttack);
    }
}
