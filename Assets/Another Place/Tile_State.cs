using System;
using System.Threading.Tasks;

using UnityEngine;

[Serializable]
public class Tile_State
{
    [SerializeField]
    private bool _active;
    public bool active
    {
        get => _active;
        set
        {
            if (_active == value) return;

            _active = value;

            if (_active)
            {
                onActive?.Invoke();

                Fill();
            }
            else
            {
                onInactive?.Invoke();

                Drain();
            }
        }
    }

    [SerializeField]
    private bool _fullyActive = false;
    public  bool fullyActive => _fullyActive;

    [SerializeField]
    private bool _fullyInactive = true;
    public bool fullyInactive => _fullyActive;
    
    public Action onActive;
    public Action onInactive;

    [SerializeField]
    private float _normal;
    protected float normal
    {
        get => _normal;
        set => _normal = Mathf.Clamp01(value);
    }

    [SerializeField]
    public float normalRate = 2.0f;

    public Action onActiveNormalStart;
    public Action<float> onActiveNormalFrame;
    public Action onActiveNormalComplete;

    public Action onInactiveNormalStart;
    public Action<float> onInactiveNormalFrame;
    public Action onInactiveNormalComplete;


    public Tile_State()
    {
        UpdateFullyActive();
        UpdateFullyInactive();
    }

    private void UpdateFullyActive() => _fullyActive = 1.0f - _normal < Mathf.Epsilon;

    private void UpdateFullyInactive() => _fullyInactive = _normal < Mathf.Epsilon;


    private async Task Fill()
    {
//        UpdateFullyInactive();

        if (_fullyInactive)
        {
            onActiveNormalStart?.Invoke();

            _fullyInactive = false;
        }

        while (_active && _normal < 1.0f)
        {
            normal += Time.deltaTime * normalRate;

            onActiveNormalFrame?.Invoke(_normal);

            await Task.Yield();
        }

        UpdateFullyActive();
        
        if (_fullyActive) onActiveNormalComplete?.Invoke();
    }

    private async Task Drain()
    {
//        UpdateFullyActive();

        if (_fullyActive)
        {
            onInactiveNormalStart?.Invoke();

            _fullyActive = false;
        }

        while (!_active &&
               _normal > 0.0f)
        {
            normal -= Time.deltaTime * normalRate;

            onInactiveNormalFrame?.Invoke(_normal);

            await Task.Yield();
        }

        UpdateFullyInactive();
        
        if (_fullyInactive) onInactiveNormalComplete?.Invoke();
    }
    
    public void FillInstant()
    {
        if (_fullyInactive)
        {
            onActiveNormalStart?.Invoke();

            _fullyInactive = false;
        }

        _normal = 1.0f;
        
        onActiveNormalFrame?.Invoke(_normal);

        _fullyActive = true;
        
        onActiveNormalComplete?.Invoke();
    }
    
    public void DrainInstant()
    {
        if (_fullyActive)
        {
            onInactiveNormalStart?.Invoke();

            _fullyActive = false;
        }

        _normal = 0.0f;
        
        onInactiveNormalFrame?.Invoke(_normal);

        _fullyInactive = true;
        
        onInactiveNormalComplete?.Invoke();
    }


    public void FlipInstant()
    {
        if (_active)
            DrainInstant();
        else
            FillInstant();
    }

}
