public abstract class BaseState : IState {

    protected EntityController controller;

    public BaseState(EntityController controller) => this.controller = controller;

    public virtual void OnEnter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnExit() { }
}