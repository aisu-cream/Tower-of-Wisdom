public abstract class BaseState : IState {

    protected EntityController controller;
    protected IEntity self;

    public BaseState(EntityController controller, IEntity self) { 
        this.controller = controller;
        this.self = self;
    }

    public virtual void OnEnter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnExit() { }
}