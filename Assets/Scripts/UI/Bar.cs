using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    [field: SerializeField]
    public float MaxValue { get; private set; }

    [field: SerializeField]
    public float CurrentValue { get; private set; }

    public float Value { get; private set; }

    [SerializeField] private RectTransform _topBar;
    [SerializeField] private RectTransform _bottomBar;
    [SerializeField] private Image _topBarImage;
    [SerializeField] private Image _bottomBarImage;
    [SerializeField] private float _animationspeed = 10f;

    [SerializeField] private Entity _entity;
    [SerializeField] private Color _highHealthColor = Color.green;
    [SerializeField] private Color _midHealthColor = Color.yellow;
    [SerializeField] private Color _lowHealthColor = Color.red;
    private float _fullWidth;
    private Coroutine _adjustBarWidthCoroutine;
    private float _previousValue;

    private float TargetWidth => Value * _fullWidth / MaxValue;

    private void Awake()
    {
        if (_entity == null)
        {
            _entity = GetComponentInParent<Entity>();
        }

        if (_entity == null)
        {
            Debug.LogError("Bar: No Entity assigned.", this);
        }
    }

    private void Start()
    {
        if (_entity == null)
        {
            Debug.LogError("Bar requires an Entity reference.", this);
            enabled = false;
            return;
        }

        MaxValue = _entity.GetInitialHealth();
        CurrentValue = _entity.GetHealth();
        Value = CurrentValue;
        _previousValue = Value;

        _fullWidth = _topBar.rect.width;
        _topBar.SetWidth(TargetWidth);
        _bottomBar.SetWidth(TargetWidth);
    }

    private void Update()
    {
        CurrentValue = _entity.GetHealth();
        Value = CurrentValue;

        if (!Mathf.Approximately(Value, _previousValue))
        {
            float amount = Value - _previousValue;
            UpdateColor();

            if (_adjustBarWidthCoroutine != null)
            {
                StopCoroutine(_adjustBarWidthCoroutine);
            }

            _adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidth(amount));
            _previousValue = Value;
        }
    }

    private IEnumerator AdjustBarWidth(float amount)
    {
        var suddenChangeBar = amount >= 0 ? _bottomBar : _topBar;
        var slowChangeBar = amount >= 0 ? _topBar : _bottomBar;

        suddenChangeBar.SetWidth(TargetWidth);

        while (Mathf.Abs(suddenChangeBar.rect.width - slowChangeBar.rect.width) > 1f)
        {
            slowChangeBar.SetWidth(
                Mathf.Lerp(slowChangeBar.rect.width, TargetWidth, Time.deltaTime * _animationspeed)
            );
            yield return null;
        }

        slowChangeBar.SetWidth(TargetWidth);
    }
    private void UpdateColor()
    {
        float percent = MaxValue <= 0f ? 0f : Value / MaxValue;


        Color targetColor = Color.Lerp(_lowHealthColor, _highHealthColor, percent);

        _topBarImage.color = targetColor;
    }
}