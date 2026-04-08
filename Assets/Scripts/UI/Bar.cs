using System;
using System.Collections;
using UnityEngine;

public class Bar : MonoBehaviour
{
    [field:SerializeField] 
    public float MaxValue { get; private set; }
    [field:SerializeField] 
    public float CurrentValue { get; private set; }
    public float Value { get; private set; }

    [SerializeField]
    private RectTransform _topBar;

    [SerializeField]
    private RectTransform _bottomBar;

    [SerializeField]
    private float _animationspeed = 10f;
    private float _fullWidth;
    private float TargetWidth => Value * _fullWidth / MaxValue;

    private Coroutine _adjustBarWidthCoroutine;
    private Entity _entity;
    private float _previousValue;
    private void Start()
    {
        _entity = GetComponent<Entity>();
        MaxValue = _entity.GetInitialHealth();
        CurrentValue = _entity.GetHealth();
        Value = CurrentValue;
        _previousValue = Value;

        _fullWidth = _topBar.rect.width;
    }
   public void SetHealth(int amount)
    {
        CurrentValue = Mathf.Clamp(CurrentValue + amount, 0, MaxValue);
        Value = CurrentValue;
        if (_adjustBarWidthCoroutine != null)
        {
            StopCoroutine(_adjustBarWidthCoroutine);
        }
        _adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidth(amount)); 
    }
    private void Update()
    {
        CurrentValue = _entity.GetHealth();
        Value = CurrentValue;
        if (Mathf.Approximately(Value, _previousValue) == false)
        {
            int amount = (int)(Value - _previousValue);
            if (_adjustBarWidthCoroutine != null)
            {
                StopCoroutine(_adjustBarWidthCoroutine);
            }
            _adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidth(amount));
            _previousValue = Value;
        }
    }
    
    private IEnumerator AdjustBarWidth(int amount)
    {
        var suddenChangeBar = amount >= 0 ? _bottomBar : _topBar;
        var slowChangeBar = amount >= 0 ? _topBar : _bottomBar;
        suddenChangeBar.SetWidth(TargetWidth);
        while (Mathf.Abs(suddenChangeBar.rect.width - slowChangeBar.rect.width) > 1f)
        {
            slowChangeBar.SetWidth(
                Mathf.Lerp(slowChangeBar.rect.width, TargetWidth, Time.deltaTime * _animationspeed));
            yield return null;
        }
        slowChangeBar.SetWidth(TargetWidth);
    }
}
