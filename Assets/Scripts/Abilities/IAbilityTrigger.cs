public interface IAbilityTrigger {
    bool CanConsumeCost();
    void ConsumeCost(EffectContext context);
    void TryExecute(EffectContext context);
}
