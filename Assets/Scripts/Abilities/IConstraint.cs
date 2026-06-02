using UnityEngine;

public interface IConstraint {
    bool Evaluate(IEntity entity);
}
